using System;
using System.Configuration;
using System.IO;

using BitfinexAPI;

namespace MannualTool
{
    class Program
    {
        static BitfinexMethod _bm;

        static void AutoTrading()
        {
            var ss = File.ReadAllLines("trade.csv");
            foreach (var i in ss)
            {
                var pars = i.Split(',');

                string symbol = pars[0];
                decimal amount = decimal.Parse(pars[1]);
                decimal price = 1;
                OrderSide side = OrderSide.BUY;
                if (amount < 0)
                {
                    amount = 0 - amount;
                    side = OrderSide.SELL;
                }
                OrderType type = OrderType.MARKET;

                try
                {
                    var result = _bm.CreateOrder(symbol, amount, price, side, type).Result;

                    Console.WriteLine("Traded:" + result.symbol
                        + "  " + result.executed_amount
                        + "  " + result.avg_execution_price);
                }
                catch (Exception e)
                {
                    Console.WriteLine("failed:" + symbol + "  " + e.Message);
                }
            }

            Console.WriteLine("Over");
        }

        static void ObtainTradeHistory()
        {
            var lists = _bm.GetOrdersHistory().Result;

            if (File.Exists("history.csv"))
                File.Delete("history.csv");

            File.AppendAllText("history.csv",
                "id,is_live,is_cancelled,symbol,side,original_amount,executed_amount,"
                + "original_price,avg_execution_price,type,timestamp\n");

            foreach (var i in lists)
            {
                string s = i.id + ","
                    + i.is_live + ","
                    + i.is_cancelled + ","
                    + i.symbol + ","
                    + i.side + ","
                    + i.original_amount + ","
                    + i.executed_amount + ","
                    + i.price.GetValueOrDefault() + ","
                    + i.avg_execution_price + ","
                    + i.type + ","
                    + i.timestamp.ToLocalTime();

                File.AppendAllText("history.csv", s + "\n");
            }

            Console.WriteLine("over");
        }

        static void MarketData(string symbol, DateTime timestamp)
        {
            var f = File.Create(symbol + ".csv");
            var writer = new StreamWriter(f);
            writer.WriteLine("timestamp,open,close,high,low,volume");

            var point = timestamp;
            while (point < DateTime.Now.Date)
            {
                var next = point + new TimeSpan(30, 0, 0, 0);
                if (next > DateTime.Now) next = DateTime.Now;

                var data = _bm.GetHistoryKlines(symbol, KlineInterval.OneHour, point, next).Result;

                foreach (var i in data)
                    writer.WriteLine(i.timestamp + ","
                        + i.open + ","
                        + i.close + ","
                        + i.high + ","
                        + i.low + ","
                        + i.volume);

                point = next;
            }

            writer.Close();
            f.Close();
        }

        static void Main(string[] args)
        {
            string apiKey = ConfigurationManager.AppSettings["ApiKey"];
            string secretKey = ConfigurationManager.AppSettings["SecretKey"];

            _bm = new BitfinexMethod(apiKey, secretKey);

            bool f = true;
            while (f)
            {
                Console.Write("command:");
                string cmd = Console.ReadLine();

                switch (cmd)
                {
                    case "AutoTrading":
                        AutoTrading();
                        break;
                    case "ObtainTradeHistory":
                        ObtainTradeHistory();
                        break;
                    case "MarketData":
                        Console.Write("symbol:");
                        var symbol = Console.ReadLine();
                        MarketData(symbol, new DateTime(2017, 1, 1));
                        break;
                    case "Exit":
                        f = false;
                        break;
                    default:
                        continue;
                }

                Console.WriteLine("--------");
            }

            Console.ReadKey();
        }
    }
}
