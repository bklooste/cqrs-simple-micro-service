using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Inventory.Feed
{
    public class Program
    {
        public static void Main(string[] args)
        {          
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
          Host.CreateDefaultBuilder(args)
              .ConfigureLogging(loggerFactory => loggerFactory.AddApplicationInsights())
              .ConfigureServices((hostContext, services) =>
              {
                  services.AddHostedService<ImportFeed>();
              });
    }
}
