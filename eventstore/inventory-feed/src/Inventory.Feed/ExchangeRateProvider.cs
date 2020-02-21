using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Inventory.Feed
{
    public class FixerHttpExchangeProvider
    {

        readonly string url = $"http://data.fixer.io/api/latest?access_key=9267dccdd4b880c3adec44a27fd36159";
        readonly HttpClient httpClient = new HttpClient();

        public FixerHttpExchangeProvider(string url)
        {
            if (string.IsNullOrEmpty(url) == false)
                this.url = url;

        }

        public async Task<IEnumerable<(string code, double rate, DateTime time)>> GetEURates()
        {
            var request = await httpClient.GetAsync(url);
            request.EnsureSuccessStatusCode();

            var content = await request.Content.ReadAsStringAsync();

            var jsonData = JObject.Parse(content);
            return ParseData(jsonData);
        }

        IEnumerable<(string code, double rate, DateTime time)> ParseData(JObject data)
        {
            var rates = data.Value<JObject>("rates");
            var unixTimeStamp = data.Value<double>("timestamp");
            var dateTime = UnixTimeStampToDateTime(unixTimeStamp);

            foreach (var rate in rates)
                yield return (rate.Key, (double)rate.Value, dateTime);
        }

        static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp);
            return dtDateTime;
        }

    }
}
