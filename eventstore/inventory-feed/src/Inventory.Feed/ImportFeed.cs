using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Inventory.Feed
{
    internal class ImportFeed: IHostedService, IDisposable
    {
        const string StreamNameBase = "feed-";
        const string EventTypeName = "exchangeRateReceived";

        readonly CancellationTokenSource stoppingCts = new CancellationTokenSource();
        readonly ILogger<ImportFeed> logger;
        readonly FixerHttpExchangeProvider exchangeRateProvider;
        readonly IEventStoreConnection connection;
        readonly IHostApplicationLifetime hostLifeTime;
        Task? executingTask;

        public ImportFeed(ILogger<ImportFeed> logger, IConfiguration configuration, IHostApplicationLifetime hostLifeTime)
        {
            this.logger = logger;
            this.exchangeRateProvider = new FixerHttpExchangeProvider(configuration["Fixer:Url"]);
            this.connection = EventStoreConnection.Create(configuration.GetConnectionString("EventStoreConnection"), "exchangefeed");
            this.hostLifeTime = hostLifeTime;
        }

        async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                foreach ( var (code, rate, time) in await exchangeRateProvider.GetEURates())
                    await SaveRateToEventStoreStream(code, rate, time);

                await Task.Delay(TimeSpan.FromMinutes(5));
                if (stoppingCts.IsCancellationRequested)
                    break;
            }
        }

        async Task SaveRateToEventStoreStream(string code, double rate, DateTime time)
        {
            var streamName = StreamNameBase + code;
            var json = JsonConvert.SerializeObject(new { Code = code, Rate = rate, Time = time });
            var jsonBytes = Encoding.UTF8.GetBytes(json);

            var eventData = new EventData(Guid.NewGuid(), EventTypeName, true, jsonBytes, null);
            await connection.AppendToStreamAsync(streamName, ExpectedVersion.Any, eventData);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Start Async");
            await connection.ConnectAsync();
            connection.Closed += (sender, e) =>
            {
                logger.LogError("Lost connection restarting service");
                hostLifeTime.StopApplication();
            };
            executingTask = ExecuteAsync(stoppingCts.Token);
            return;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stop Async");

            if (executingTask == null)
                return;

            try
            {
                connection.Close();
                stoppingCts.Cancel();
            }
            finally
            {
                await Task.WhenAny(executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }

        public void Dispose()
        {
            stoppingCts.Cancel();
        }
    }
}