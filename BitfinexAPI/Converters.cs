using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace BitfinexAPI
{
    static class ConvertHelper
    {
        public static string ObtainEnumValue<T>(T data)
        {
            var info = JsonConvert.SerializeObject(data, new StringEnumConverter());
            return JsonConvert.DeserializeObject<string>(info);
        }
    }

    class V1TimeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return DateTimeOffset.FromUnixTimeSeconds((long)decimal.Parse(reader.Value.ToString())).DateTime;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(new DateTimeOffset((DateTime)value).ToUnixTimeSeconds());
        }
    }

    class KlineConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var o = JArray.Load(reader);

            return new KlineInfo()
            {
                timestamp = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(o[0])).DateTime,
                open = Convert.ToDecimal(o[1]),
                close = Convert.ToDecimal(o[2]),
                high = Convert.ToDecimal(o[3]),
                low = Convert.ToDecimal(o[4]),
                volume = Convert.ToDecimal(o[5]),
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    class TradeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var o = JArray.Load(reader);
            // The first two condtions deal with the scenerio occrued in websocket
            if (o[1] is JArray)
                return new TradeRecordInfo()
                {
                    id = Convert.ToInt64(o[1][0][0]),
                    timestamp = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(o[1][0][1])).DateTime,
                    amount = Convert.ToDecimal(o[1][0][2]),
                    price = Convert.ToDecimal(o[1][0][3]),
                };
            else if (Convert.ToString(o[1]) == "tu")
                return new TradeRecordInfo()
                {
                    id = Convert.ToInt64(o[2][0]),
                    timestamp = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(o[2][1])).DateTime,
                    amount = Convert.ToDecimal(o[2][2]),
                    price = Convert.ToDecimal(o[2][3]),
                };
            else
                return new TradeRecordInfo()
                {
                    id = Convert.ToInt64(o[0]),
                    timestamp = DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(o[1])).DateTime,
                    amount = Convert.ToDecimal(o[2]),
                    price = Convert.ToDecimal(o[3]),
                };

        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    class PairInfoConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var o = JArray.Load(reader);
            if (o.Count == 3)
            {
                return new PairInfo()
                {
                    amount = Convert.ToDecimal(o[2]),
                    price = Convert.ToDecimal(o[0]),
                    timestamp = DateTime.Now
                };
            }
            else
            {
                return new PairInfo()
                {
                    amount = Convert.ToDecimal(o[3]),
                    price = Convert.ToDecimal(o[1]),
                    timestamp = DateTime.Now
                };
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
    class OrderBookConverter: JsonConverter
        {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var o = JArray.Load(reader);
            string tempPairInfos = o[1].ToString();
            List<PairInfo> pairInfos = JsonConvert.DeserializeObject<List<PairInfo>>(tempPairInfos);
            List<PairInfo> _asks = new List<PairInfo>();
            List<PairInfo> _bids = new List<PairInfo>();
            foreach (PairInfo pairInfo in pairInfos)
            {
                if (pairInfo.amount >= 0)
                    _bids.Add(pairInfo);
                else
                    _asks.Add(pairInfo);     
            }
            return new OrderBookInfo()
            {
                asks = _asks,
                bids = _bids
            };
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
        }
}


