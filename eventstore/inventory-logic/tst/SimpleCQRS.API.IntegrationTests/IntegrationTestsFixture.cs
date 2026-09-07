using System;

using Microsoft.Extensions.Configuration;

using EventStore.Client;
using System.Collections.Generic;

namespace SimpleCQRS.API.IntegrationTest
{
    public class IntegrationTestFixture : IDisposable
    {
        readonly IConfiguration config;

        public EventStoreClient StoreConnection { get; }
        public int Port=> int.Parse(config["InventoryLogicServicePort"]);


        public IntegrationTestFixture()
        {
            var configDefaults = new Dictionary<string, string>
            {
                {"ConnectionStrings:EventStoreConnection", "esdb://admin:changeit@127.0.0.1:2114?tls=false"},
                {"InventoryLogicServicePort", "53104"}
            };

            this.config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDefaults)
                .AddEnvironmentVariables()
                .Build();

            var connection = config["ConnectionStrings:EventStoreConnection"];

            var settings = EventStoreClientSettings.Create(connection);
            settings.ConnectionName = "integrationTests";
            this.StoreConnection = new EventStoreClient(settings);
        }

        public void Dispose()
        {
            StoreConnection.Dispose();
        }
    }
}
