#! "netcoreapp2.0"
#r "nuget:BenchmarkDotNet, 0.10.12"
#r "nuget:Newtonsoft.Json, 12.0.1"

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Running;
using Newtonsoft.Json;


BenchmarkRunner.Run<JsonTest>();

 

// dotnet script .\main.csx -c Release

    public class MetaData 
    {
        public Guid SiteId { get; set;}
        public Guid NodeId { get; set;}
        public DateTime Time { get; set; }

        public bool IsOOO { get; set;} // default to false
        public bool DisableDown { get; set;} // default to false
        public bool DisableUp { get; set; } // default to false

        public Guid? OOOOriginalEventId { get; set;}
    }


[InProcess]
public class JsonTest
{       
    byte[] values = new byte[48] ;
    string json;

    public JsonTest()
    {
        values[0] = 48;
        values[18] = 18;
        var metaData = new MetaData(){SiteId = Guid.NewGuid() , IsOOO = true, DisableDown = true, NodeId = Guid.NewGuid() ,Time = DateTime.Now };
        json = (string) JsonConvert.SerializeObject(metaData);
    }
 
    [Benchmark]
    public void Binary()
    {
        var data  = new Guid(values.Skip(16).Take(16).ToArray());
    }

   
}