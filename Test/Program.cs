using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net.Http;

using BitfinexAPI;
using BinanceAPI;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp;
using Newtonsoft.Json;

namespace BitfinexAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            /*
            string apiKey = ConfigurationManager.AppSettings["ApiKey"];
            string secretKey = ConfigurationManager.AppSettings["SecretKey"];
            OrderBookInfo BTC = new OrderBookInfo();
            BitfinexMethod method = new BitfinexMethod("","");
            BTC = method.GetSnapShot_OrderBook("tBTCUSD", "tBTCUSD");
            method.InstantUpdate_OrderBook("tBTCUSD");
            //Main thread to update the info at a constant frequncy
            while (true)
            {
                Thread.Sleep(1000);
                Console.WriteLine("The current price of BTC is " + BTC.asks[0].price);
            }
            */
            string tradeInfo = "[75, \"tu\",  [249930637, 1527253236365, -0.6448805, 7501.3]]";
            TradeRecordInfo info = JsonConvert.DeserializeObject<TradeRecordInfo>(tradeInfo);
            Console.WriteLine(string.Format("The price is {0}, the amount is {1}, the id is {2}, the date is {3}",info.price,info.amount,info.id,info.timestamp));
            Console.ReadKey();
        }

    }
}
