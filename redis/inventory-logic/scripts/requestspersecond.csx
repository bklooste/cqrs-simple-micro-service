#! "netcoreapp3.0"
#r "nuget:BenchmarkDotNet, 0.10.12"
#r "nuget:Newtonsoft.Json, 12.0.1"

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Running;
using Newtonsoft.Json;
using System.Net.Http;

// dotnet script .\main.csx -c Release
BenchmarkRunner.Run<HttpTest>();

[InProcess]
public class HttpTest
{       
    byte[] values = new byte[48] ;
    string json;

    public HttpTest()
    {

    }
 
    [Benchmark]
    public void Binary()
    {
        var handler = new HttpClientHandler
        {
            MaxConnectionsPerServer= 100
        };

        var client = new HttpClient(handler);


        var id = Guid.NewGuid();
        var url = $"http://localhost:54105/InventoryCommand/Add?name=name{id.ToString()}";
        client.PostAsync(url, null).Wait();
    }

   
}