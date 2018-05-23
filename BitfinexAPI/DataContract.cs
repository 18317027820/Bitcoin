using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BitfinexAPI
{
    public class BaseInfo : Dictionary<string, object>
    {
    }

    public class TickerInfo
    {
        public decimal mid;
        public decimal bid;
        public decimal ask;
        public decimal last_price;
        public decimal low;
        public decimal high;
        public decimal volume;
        [JsonConverter(typeof(V1TimeConverter))]
        public DateTime timestamp;
    }

    [JsonConverter(typeof(PairInfoConverter))]
    public class PairInfo
    {
        public decimal price;
        public decimal amount;
        public DateTime timestamp;
    }

    [JsonConverter(typeof(OrderBookConverter))]
    public class OrderBookInfo
    {
        public List<PairInfo> asks;  //Positive values mean bid, negative values mean ask.
        public List<PairInfo> bids;
        public void Update(PairInfo pairInfo)
        {
            int i = 0;
            if (pairInfo.amount < 0)
            {
                foreach (PairInfo temp in asks)
                {
                    if (pairInfo.price < temp.price)
                    {
                        break;
                    }
                    i++;
                }
                if (i < asks.Count)
                {
                    asks.Insert(i, pairInfo);
                    asks.RemoveAt(asks.Count - 1);
                }
            }
            else
            {
                foreach (PairInfo temp in bids)
                {
                    if (pairInfo.price > temp.price)
                    {
                        break;
                    }
                    i++;
                }

                if (i < bids.Count)
                {
                    bids.Insert(i, pairInfo);
                    bids.RemoveAt(asks.Count - 1);
                }
            }
        }
    }

    
    public class TradeInfo : PairInfo
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public OrderSide type;
        public long tid;
        public string exchange;
    }
    

    public class TransactionInfo : TradeInfo
    {
        public string fee_currency;
        public decimal fee_amount;
        public long order_id;
    }
    
    public class OrderInfo
    {
        public long id;
        public string symbol;
        public string exchange;
        public decimal? price;
        public decimal avg_execution_price;
        [JsonConverter(typeof(StringEnumConverter))]
        public OrderSide side;
        [JsonConverter(typeof(StringEnumConverter))]
        public OrderType type;
        [JsonConverter(typeof(V1TimeConverter))]
        public DateTime timestamp;
        public bool is_live;
        public bool is_cancelled;
        public bool is_hidden;
        public bool was_forced;
        public decimal original_amount;
        public decimal remaining_amount;
        public decimal executed_amount;
    }

    public class PositionInfo
    {
        public long id;
        public string symbol;
        public string status;
        [JsonProperty("base")]
        public decimal base_price;
        public decimal amount;
        [JsonConverter(typeof(V1TimeConverter))]
        public DateTime timestamp;
        public decimal swap;
        public decimal pl;
    }

    public class BalanceInfo
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public WalletType type;
        public string currency;
        public decimal amount;
        public decimal available;
    }

    public class AssetMovementInfo
    {
        public long id;
        public string txid;
        public string currency;
        public string method;
        public string type;
        public decimal amount;
        public string description;
        public string address;
        public string status;
        [JsonConverter(typeof(V1TimeConverter))]
        public DateTime timestamp;
        [JsonConverter(typeof(V1TimeConverter))]
        public DateTime timestamp_created;
        public decimal fee;
    }

    [JsonConverter(typeof(KlineConverter))]
    public class KlineInfo
    {
        public DateTime timestamp;
        public decimal open;
        public decimal close;
        public decimal high;
        public decimal low;
        public decimal volume;
    }

    [JsonConverter(typeof(TradeConverter))]
    public class TradeRecordInfo
    {
        public long id;
        public DateTime timestamp;
        public decimal price;
        public decimal amount;
    }
}
