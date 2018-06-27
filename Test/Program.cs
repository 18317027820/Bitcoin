using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using BitfinexAPI;
using SqlUtility;
using WebSocketSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using MySql.Data.MySqlClient;
using System.Timers;

namespace Test
{
    class MyTimer:Timer
    {
        public DateTime start;
        public KlineInterval klineInterval;
        public DateTime preStart;
    }

    
    class Program
    {

        static List<string> codes = new List<string> { "BTCUSD","BCHUSD","EOSUSD","ETHUSD","IOTUSD","LTCUSD","XMRUSD","NEOUSD","OMGUSD","XRPUSD","ZECUSD"};
        static void InitialUpdate(KlineInterval klineInterval, DateTime start,DateTime end)//local time
        {
            List<Task> tasks = new List<Task>();
            foreach (string code in codes)
            {
                tasks.Add(new Task(() => {
                    DataBuffer buffer = new DataBuffer(code, klineInterval, start, end);
                    Console.WriteLine("Start to request data " + code);
                    buffer.RequestData();
                    Console.WriteLine("Start to write data " + code);
                    buffer.WriteData();
                }));
            }
            for (int i = 0; i < tasks.Count; i++)
            {
                tasks[i].Start();
            }

            for (int i = 0; i < tasks.Count; i++)
            {
                tasks[i].Wait();
            }

        }
        static void RunUpdate(MyTimer timer,KlineInterval klineInterval,DateTime start)//local time
        {
            DateTime end = DateTime.Now;
            timer.start = end;

            
            List<Task> tasks = new List<Task>();
            foreach (string code in codes)
            {
                tasks.Add(new Task(() => {
                    DataBuffer buffer = new DataBuffer(code, klineInterval, start,end );
                    Console.WriteLine("Start to request data "+code);
                    buffer.RequestData();
                    Console.WriteLine("Start to write data "+code);
                    buffer.WriteData();
                }));
            }
            for (int i = 0; i < tasks.Count; i++)
            {
                tasks[i].Start();
            }

            for (int i = 0; i < tasks.Count; i++)
            {
                tasks[i].Wait();
            }

            timer.preStart = timer.start;           

        }
        static void SaveJson(Dictionary<string,DateTime> pairs,string filePath)
        {
            FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(fs);
            string json = JsonConvert.SerializeObject(pairs);
            writer.Write(json);
            writer.Close();
            fs.Close();
        }
        static Dictionary<string, DateTime> LoadJson(string filePath)
        {
            FileStream fs = File.Open("StartTime.json", FileMode.Open);
            StreamReader reader = new StreamReader(fs);

            Dictionary<string, DateTime>  startTime = JsonConvert.DeserializeObject<Dictionary<string, DateTime>>(reader.ReadLine());
            reader.Close();
            fs.Close();
            return startTime;
        }
        static void Main(string[] args)
        {
            string recordsPath = "StartTime.json";
            List<MyTimer> timers = new List<MyTimer>();
            List<TimeSpan> intervals = new List<TimeSpan> { new TimeSpan(0, 0, 1, 0), new TimeSpan(0, 0, 5, 0), new TimeSpan(0, 0, 15, 0), new TimeSpan(0, 0, 30, 0), new TimeSpan(0, 1, 0, 0), new TimeSpan(0, 3, 0, 0), new TimeSpan(0, 6, 0, 0), new TimeSpan(0, 12, 0, 0), new TimeSpan(1, 0, 0, 0) };
            List<KlineInterval> klineIntervals = new List<KlineInterval> { KlineInterval.OneMinute, KlineInterval.FiveMinutes, KlineInterval.FifteenMinutes, KlineInterval.ThirtyMinutes, KlineInterval.OneHour, KlineInterval.ThreeHours, KlineInterval.SixHours, KlineInterval.TwelveHours, KlineInterval.OneDay };
            Dictionary<string, DateTime> startTime = LoadJson(recordsPath);
            DateTime temp;


            //Initial Upadate since it takes much longer than timed updates.
            for (int i = 0; i < intervals.Count; i++)   
            {
                temp = DateTime.Now;
                InitialUpdate(klineIntervals[i], startTime[klineIntervals[i].ToString()]-new TimeSpan(0, 0, (int)(intervals[i].TotalSeconds * 20) ), temp);
                startTime[klineIntervals[i].ToString()] = temp;
                SaveJson(startTime, recordsPath);
            }


            for (int i = 0; i < intervals.Count; i++)
            {
                timers.Add(new MyTimer());
                timers[i].klineInterval = klineIntervals[i];
                timers[i].start = startTime[klineIntervals[i].ToString()];
                timers[i].preStart= startTime[klineIntervals[i].ToString()];
                timers[i].Interval = intervals[i].TotalMilliseconds;
                timers[i].AutoReset = true;
                timers[i].Elapsed += (sender, e) => {   Console.WriteLine(((MyTimer)sender).start); Console.WriteLine(((MyTimer)sender).klineInterval); RunUpdate((MyTimer)sender,((MyTimer)sender).klineInterval, ((MyTimer)sender).start); };
                timers[i].Start();
            }
            Console.WriteLine("Set Finished!");
            while (Console.ReadLine() != "exit") ;


            //Update startTime for each klines
            foreach (MyTimer timer in timers)   
            {
                timer.Stop();
                startTime[timer.klineInterval.ToString()]= timer.preStart;
            }

            SaveJson(startTime, recordsPath);

        }

    }

}

