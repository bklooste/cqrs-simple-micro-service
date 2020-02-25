using System;

using Microsoft.Extensions.Configuration;

using System.Collections.Generic;
using StackExchange.Redis;

namespace SimpleCQRS.Views.IntegrationTest
{
    public class IntegrationTestFixture : IDisposable
    {
        readonly IConfiguration config;
        readonly ConnectionMultiplexer redis;

        public IDatabase StoreConnection { get; }
        public int Port=> int.Parse(config["InventoryViewsServicePort"]);

        //54106
        public IntegrationTestFixture() 
        {
            var configDefaults = new Dictionary<string, string>
            {
                {"ConnectionStrings:RedisConnection", "127.0.0.1:6479,allowAdmin=false"},
                {"InventoryViewsServicePort", "53106"}
            };

            this.config = new ConfigurationBuilder()
                .AddInMemoryCollection(configDefaults)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = config["ConnectionStrings:RedisConnection"];
            this.redis = ConnectionMultiplexer.Connect(connectionString);

            this.StoreConnection = redis.GetDatabase();
        }

        public void Dispose()
        {
            this.redis.Dispose();
        }
    }
}
