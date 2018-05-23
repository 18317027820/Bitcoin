using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using WebSocketSharp;

namespace BitfinexAPI
{
    
    static class AccessWebSocket
    {
        const string endpointBase = "wss://api.bitfinex.com/ws";

        public static Dictionary<string, WebSocket> _socketPool;
        public static Dictionary<string, Queue<string>> _bufferPool;
        public static Dictionary<string, object> _snapShotPool;

        static AccessWebSocket()
        {
            _socketPool = new Dictionary<string, WebSocket>();
            _bufferPool = new Dictionary<string, Queue<string>>();
            _snapShotPool = new Dictionary<string, object>();
        }

        public static string Subscribe<T>(string args, string _chanId)
        {
            WebSocket ws = new WebSocket(endpointBase + args);
            string chanId = _chanId;
            ws.SetProxy("http://localhost:1080", null, null);
            ws.OnOpen += (sender, message) =>
             {
                 Console.WriteLine("Connect Success!!");
                 _socketPool.Add(chanId, ws);
                 _bufferPool.Add(chanId,new Queue<string>());
                 _snapShotPool.Add(chanId, new object());

             };

            ws.OnMessage += (sender, message) =>
            {
                _bufferPool[chanId].Enqueue(message.Data);
            };

            ws.OnError += (sender, error) =>
            {
                throw new Exception("WebSocketException:" + typeof(T).FullName);
            };

            ws.Connect();
            ws.Send(args);

            return chanId;
        }

        public static void Unsubscribe(string chanId)
        {
            _socketPool[chanId].Close();
            _socketPool.Remove(chanId);
        }
    }
    
}
