using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitfinexAPI;
using MySql.Data.MySqlClient;
using System.Threading;
using Newtonsoft.Json;

namespace SqlUtility
{
    public class DataBuffer
        {
            public string sqlTemplate = "insert into klines.{0} (`Code`,`Time`,`Interval`,`Open`,`Close`,`High`,`Low`,`Volume`) values (\"{1}.BFX\",\"{2}\",{3},{4},{5},{6},{7},{8});";
            public Dictionary<string, TimeSpan> timeSpan = new Dictionary<string, TimeSpan> { { "OneMinute", new TimeSpan(0, 0, 1, 0) }, { "FiveMinutes", new TimeSpan(0, 0, 5, 0) }, { "FifteenMinutes", new TimeSpan(0, 0, 15, 0) }, { "ThirtyMinutes", new TimeSpan(0, 0, 30, 0) }, { "OneHour", new TimeSpan(0, 1, 0, 0) }, { "ThreeHours", new TimeSpan(0, 3, 0, 0) }, { "SixHours", new TimeSpan(0, 6, 0, 0) }, { "TwelveHours", new TimeSpan(0, 12, 0, 0) }, { "OneDay", new TimeSpan(1, 0, 0, 0) } };
            public Dictionary<string, int> time = new Dictionary<string, int> { { "OneMinute", 1 }, { "FiveMinutes", 5 }, { "FifteenMinutes", 15 }, { "ThirtyMinutes", 30 }, { "OneHour", 1 }, { "ThreeHours", 3 }, { "SixHours", 6 }, { "TwelveHours", 12 }, { "OneDay", 1 } };
            public Queue<KlineInfo> data = new Queue<KlineInfo>();
            public DateTime start;
            public DateTime end;
            public string code;
            public KlineInterval interval;
            public MySqlConnection connection;
            public string table;


            public DataBuffer(string _code, KlineInterval _interval, DateTime _start, DateTime _end)
            {
                code = _code;
                start = _start-timeSpan[_interval.ToString()];
                interval = _interval;
                end = _end - timeSpan[_interval.ToString()];
                connection = new MySqlConnection("Server = intelpoints.com; Port = 3306; Database = klines; Uid = DB_Admin; Pwd = vFund; ");
                if (interval.ToString().Contains("Hour")) table = "hour";
                else if (interval.ToString().Contains("Minute")) table = "minute";
                else table = "daily";
            }
            public void RequestData()
            {

                var point = start;
                while (point < end)
                {
                    var next = point + new TimeSpan(0, 0, 0, (int)timeSpan[interval.ToString()].TotalSeconds * 750);//timespan

                    try
                    {
                        BitfinexMethod _bm = new BitfinexMethod("", "");
                        var dataRequested = _bm.GetHistoryKlines(code, interval,
                                 point, (next < end) ? next : end).Result;
                        foreach (KlineInfo klineInfo in dataRequested)
                        {
                            data.Enqueue(klineInfo);
                        }
                    }

                    catch
                    {
                        Console.WriteLine("Error!");
                        Thread.Sleep(60000);
                        continue;
                    }


                    point = next;
                }
            }
            public void WriteData()
            {
                string sqlString;
                KlineInfo klineInfo;
                MySqlCommand cmd;
                while (data.Count > 0)
                {
                    klineInfo = data.Dequeue();
                    sqlString = String.Format(sqlTemplate,
                                 table,
                                 code,
                                 klineInfo.timestamp.ToLocalTime(),
                                 time[interval.ToString()],
                                 klineInfo.open,
                                 klineInfo.close,
                                 klineInfo.high,
                                 klineInfo.low,
                                 klineInfo.volume
                                 );

                    TryExecuteSql(sqlString, connection);
                }



            }
            void TryExecuteSql(string sql, MySqlConnection connection)
            {
                MySqlCommand cmd;
                while (true)
                {

                    while (true)
                    {
                        try
                        { if (connection.State != ConnectionState.Open) { connection.Open(); } break; }
                        catch (MySqlException error)
                        {
                            Console.WriteLine(error.Message);
                            Thread.Sleep(10000);
                            continue;
                        }
                    }


                try
                { cmd = new MySqlCommand(sql, connection); cmd.ExecuteNonQuery(); break; }


                catch (MySqlException e)
                {
                    if (e.Message.Contains("Duplicate entry"))
                    {
                        Console.WriteLine(e.Message); break;
                    }
                }

                catch (NullReferenceException e)
                {
                    Console.WriteLine(e.Message);
                }

                finally
                {
                    connection.Close();
                }


                }
            }

        }
}
