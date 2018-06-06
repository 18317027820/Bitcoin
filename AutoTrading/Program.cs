using System;
using System.Configuration;
using System.IO;

using BitfinexAPI;

namespace AutoTrading
{
    class Program
    {
        static void Main(string[] args)
        {
            string apiKey = ConfigurationManager.AppSettings["ApiKey"];
            string secretKey = ConfigurationManager.AppSettings["SecretKey"];

            BitfinexMethod bm = new BitfinexMethod(apiKey, secretKey);

            var ss = File.ReadAllLines("trade.csv");
            foreach (var i in ss)
            {
                var pars = i.Split(',');

                string symbol = pars[0];
                decimal amount = decimal.Parse(pars[1]);
                decimal price = decimal.Parse("1");
                OrderSide side = OrderSide.BUY;
                if (amount < 0)
                {
                    amount = 0 - amount;
                    side = OrderSide.SELL;
                }
                OrderType type = OrderType.MARKET;

                var result = bm.CreateOrder(symbol, amount, price, side, type).Result;
            }

            Console.ReadKey();
        }
    }
}
