using System;

using Microsoft.Extensions.Configuration;

using EventStore.Client;
using System.Collections.Generic;

namespace SimpleCQRS.Views.IntegrationTest
{
    public class IntegrationTestFixture : IDisposable
    {
        readonly IConfiguration config;

        public EventStoreClient StoreConnection { get; }
        public int Port=> int.Parse(config["InventoryViewsServicePort"]);


        public IntegrationTestFixture()
        {
            var configDefaults = new Dictionary<string, string>
            {
                {"ConnectionStrings:EventStoreConnection", "esdb://admin:changeit@127.0.0.1:2115?tls=false"},
                {"InventoryViewsServicePort", "53105"}
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
