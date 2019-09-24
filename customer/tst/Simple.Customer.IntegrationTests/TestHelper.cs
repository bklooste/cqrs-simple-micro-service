using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Simple.Customers.IntegrationTest
{
    static class TestHelper
    {
        public static async Task RunUntilPassed(Func<Task<bool>> isOk)
        {
            for (var i = 0; i < 20; i++)
            {
                if (i == 20)
                    throw new Exception($"20 tries giving up");
                try
                {
                    if (await isOk())
                        break;
                }
                catch (HttpRequestException)
                {
                }

                Thread.Sleep(i * i * 200);
            }
        }

        public static void BlockGetTillAvailable(this HttpClient client, string url)
        {
            Task.Run( async () => await RunUntilPassed(async () =>
            {
                var status = await client.GetAsync(url); 
                if (status.IsSuccessStatusCode)
                    return true;

                return false;
            })).Wait();
        }

    }
}
