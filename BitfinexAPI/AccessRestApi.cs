using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace BitfinexAPI
{
    static class AccessRestApi
    {
        const string endpointBase = "https://api.bitfinex.com";

        public static async Task<T> InvokeHttpCall<T>(string path)
        {
            return await InvokeHttpCall<T>(path, null, "", "");
        }

        public static async Task<T> InvokeHttpCall<T>(
            string path, BaseInfo args, string apiKey, string secretKey)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, endpointBase + path);

            if (args != null)
            {
                req.Method = HttpMethod.Post;

                string json = JsonConvert.SerializeObject(args);
                string payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

                var crypto = new HMACSHA384(Encoding.UTF8.GetBytes(secretKey));
                var hash = crypto.ComputeHash(Encoding.UTF8.GetBytes(payload));
                string signature = BitConverter.ToString(hash).Replace("-", "").ToLower();

                req.Headers.Add("X-BFX-APIKEY", apiKey);
                req.Headers.Add("X-BFX-PAYLOAD", payload);
                req.Headers.Add("X-BFX-SIGNATURE", signature);
            }

            var res = await (new HttpClient()).SendAsync(req);

            string data = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                throw new HttpRequestException(data);

            return JsonConvert.DeserializeObject<T>(data);
        }
    }
}
