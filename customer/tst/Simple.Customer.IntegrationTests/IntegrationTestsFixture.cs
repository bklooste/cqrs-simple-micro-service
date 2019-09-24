using System;

using Microsoft.Extensions.Configuration;

using EventStore.ClientAPI;
using System.Collections.Generic;

namespace Simple.Customers.IntegrationTest
{
    public class IntegrationTestFixture : IDisposable
    {
        readonly IConfiguration config;

        public IEventStoreConnection StoreConnection { get; }
        public int Port=> int.Parse(config["InventoryLogicServicePort"]);


        public IntegrationTestFixture() 
        {
            var configDefaults = new Dictionary<string, string>
            {
                {"ConnectionStrings:EventStoreConnection", "ConnectTo=tcp://admin:changeit@127.0.0.1:1114"},
                {"InventoryLogicServicePort", "53104"}
            };

            this.config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDefaults)
                .AddEnvironmentVariables()
                .Build();

            var connection = config["ConnectionStrings:EventStoreConnection"];

            this.StoreConnection = EventStoreConnection.Create(connection, "integrationTests");
            this.StoreConnection.ConnectAsync().Wait();
        }

        public void Dispose()
        {
            StoreConnection.Dispose();
        }
    }
}
