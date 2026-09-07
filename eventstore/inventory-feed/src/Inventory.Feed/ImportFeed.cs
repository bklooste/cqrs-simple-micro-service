using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using EventStore.Client;
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
        readonly EventStoreClient connection;
        Task? executingTask;

        public ImportFeed(ILogger<ImportFeed> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.exchangeRateProvider = new FixerHttpExchangeProvider(configuration["Fixer:Url"]);
            var settings = EventStoreClientSettings.Create(configuration.GetConnectionString("EventStoreConnection"));
            settings.ConnectionName = "exchangefeed";
            this.connection = new EventStoreClient(settings);
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

            var eventData = new EventData(Uuid.NewUuid(), EventTypeName, jsonBytes);
            await connection.AppendToStreamAsync(streamName, StreamState.Any, new[] { eventData });
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Start Async");
            executingTask = ExecuteAsync(stoppingCts.Token);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Stop Async");

            if (executingTask == null)
                return;

            try
            {
                stoppingCts.Cancel();
            }
            finally
            {
                await Task.WhenAny(executingTask, Task.Delay(System.Threading.Timeout.Infinite, cancellationToken));
            }
        }

        public void Dispose()
        {
            stoppingCts.Cancel();
            connection.Dispose();
        }
    }
}
