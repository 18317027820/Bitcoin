using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BitfinexAPI
{
    public class BitfinexMethod
    {
        string _apiKey;
        string _secretKey;

        public BitfinexMethod(string apiKey, string secretKey)
        {
            _apiKey = apiKey;
            _secretKey = secretKey;
        }

        BaseInfo GeneratePayload(string path)
        {
            var args = new BaseInfo();
            args.Add("request", path);
            args.Add("nonce", DateTime.Now.Ticks.ToString());

            return args;
        }

        async Task<T> ProcessPublic<T>(string path)
        {
            return await AccessRestApi.InvokeHttpCall<T>(path);
        }

        async Task<T> ProcessAuthenticated<T>(BaseInfo args)
        {
            return await AccessRestApi.InvokeHttpCall<T>(
                (string)args["request"], args, _apiKey, _secretKey);
        }

        public async Task<List<string>> GetSymbols()
        {
            return await ProcessPublic<List<string>>("/v1/symbols");
        }

        public async Task<TickerInfo> GetTicker(string symbol)
        {
            return await ProcessPublic<TickerInfo>("/v1/pubticker/" + symbol.ToLower());
        }

        public async Task<List<TradeInfo>> GetTrades(string symbol)
        {
            return await ProcessPublic<List<TradeInfo>>("/v1/trades/" + symbol.ToLower());
        }

        public async Task<OrderBookInfo> GetOrderBook(string symbol)
        {
            return await ProcessPublic<OrderBookInfo>("/v1/book/" + symbol.ToLower());
        }

        public async Task<List<BalanceInfo>> GetBalances()
        {
            var args = GeneratePayload("/v1/balances");
            return await ProcessAuthenticated<List<BalanceInfo>>(args);
        }

        public async Task<List<OrderInfo>> GetActiveOrders()
        {
            var args = GeneratePayload("/v1/orders");
            return await ProcessAuthenticated<List<OrderInfo>>(args);
        }

        public async Task<List<OrderInfo>> GetOrdersHistory()
        {
            var args = GeneratePayload("/v1/orders/hist");
            return await ProcessAuthenticated<List<OrderInfo>>(args);
        }

        public async Task<List<PositionInfo>> GetActivePositions()
        {
            var args = GeneratePayload("/v1/positions");
            return await ProcessAuthenticated<List<PositionInfo>>(args);
        }

        public async Task<List<TransactionInfo>> GetTradeRecords(string symbol)
        {
            var args = GeneratePayload("/v1/mytrades");
            args.Add("symbol", symbol.ToLower());

            return await ProcessAuthenticated<List<TransactionInfo>>(args);
        }

        public async Task<List<AssetMovementInfo>> GetAssetMovements(string currency)
        {
            var args = GeneratePayload("/v1/history/movements");
            args.Add("currency", currency.ToUpper());

            return await ProcessAuthenticated<List<AssetMovementInfo>>(args);
        }

        public async Task<List<BaseInfo>> TransferWallets(
            decimal amount,
            string currency,
            WalletType walletfrom,
            WalletType walletto)
        {
            var args = GeneratePayload("/v1/transfer");
            args.Add("amount", amount.ToString());
            args.Add("currency", currency.ToUpper());
            args.Add("walletfrom", ConvertHelper.ObtainEnumValue(walletfrom));
            args.Add("walletto", ConvertHelper.ObtainEnumValue(walletto));

            return await ProcessAuthenticated<List<BaseInfo>>(args);
        }

        public async Task<OrderInfo> CreateOrder(
            string symbol,
            decimal amount,
            decimal price,
            OrderSide side,
            OrderType type)
        {
            var args = GeneratePayload("/v1/order/new");
            args.Add("exchange", "bitfinex");
            args.Add("symbol", symbol.ToLower());
            args.Add("amount", amount.ToString());
            args.Add("price", price.ToString());
            args.Add("side", ConvertHelper.ObtainEnumValue(side));
            args.Add("type", ConvertHelper.ObtainEnumValue(type));

            return await ProcessAuthenticated<OrderInfo>(args);
        }

        public async Task<BaseInfo> CancelAllOrders()
        {
            var args = GeneratePayload("/v1/order/cancel/all");
            return await ProcessAuthenticated<BaseInfo>(args);
        }

        public async Task<OrderInfo> CancelOrder(long id)
        {
            var args = GeneratePayload("/v1/order/cancel");
            args.Add("order_id", id);

            return await ProcessAuthenticated<OrderInfo>(args);
        }

        public async Task<BaseInfo> ClosePosition(long id)
        {
            var args = GeneratePayload("/v1/position/close");
            args.Add("position_id", id);

            return await ProcessAuthenticated<BaseInfo>(args);
        }

        public async Task<List<KlineInfo>> GetHistoryKlines(
            string symbol,
            KlineInterval interval,
            DateTime start,
            DateTime end,
            int limit = 800)
        {
            long s = new DateTimeOffset(start).ToUnixTimeMilliseconds();
            long e = new DateTimeOffset(end).ToUnixTimeMilliseconds();

            string path = "/v2/candles/trade:"
                + ConvertHelper.ObtainEnumValue(interval)
                + ":t" + symbol.ToUpper()
                + "/hist?limit=" + limit.ToString()
                + "&start=" + s.ToString()
                + "&end=" + e.ToString()
                + "&sort=1";

            return await ProcessPublic<List<KlineInfo>>(path);
        }

        public async Task<List<TradeRecordInfo>> GetHistoryTrades(
            string symbol,
            DateTime start,
            DateTime end,
            int limit = 800)
        {
            long s = new DateTimeOffset(start).ToUnixTimeMilliseconds();
            long e = new DateTimeOffset(end).ToUnixTimeMilliseconds();

            string path = "/v2/trades/t" + symbol.ToUpper()
                + "/hist?limit=" + limit.ToString()
                + "&start=" + s.ToString()
                + "&end=" + e.ToString()
                + "&sort=1";

            return await ProcessPublic<List<TradeRecordInfo>>(path);
        }

        T GetSnapShot<T> (string args, string chanId)
        {
            string id = AccessWebSocket.Subscribe<T>(args,chanId);
            int messageNum = 0;
            Queue<string> buffer = AccessWebSocket._bufferPool[id];
            while (messageNum < 2)
            { 
                while (buffer.Count == 0) ;
                buffer.Dequeue();
                messageNum++;
            }
            while (buffer.Count == 0) ;
            return JsonConvert.DeserializeObject<T>(buffer.Dequeue());
        }

        public OrderBookInfo GetSnapShot_OrderBook(string symbol,string id)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("event","subscribe");
            args.Add("channel","book");
            args.Add("symbol",symbol);
            args.Add("prec","P0");
            args.Add("freq", "F0");
            args.Add("len","25");
            string request = JsonConvert.SerializeObject(args);
            OrderBookInfo res = GetSnapShot<OrderBookInfo>(request, symbol);
            AccessWebSocket._snapShotPool[id] = res;
            return res;
        }

        async public void InstantUpdate_OrderBook(string id)
        {
            Queue<string> buffer = AccessWebSocket._bufferPool[id];
            OrderBookInfo snapShot = (OrderBookInfo)AccessWebSocket._snapShotPool[id];
            Task tsk = new Task(() => { while (buffer.Count == 0) ; snapShot.Update(JsonConvert.DeserializeObject<PairInfo>(buffer.Dequeue())); });
            await tsk;
        }

    }
}
