using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;



using StackExchange.Redis;

namespace SimpleCQRS.API.IntegrationTest
{
    public class IntegrationTestFixture : IDisposable
    {
        readonly IConfiguration config;
        private ConnectionMultiplexer redis;

        public IDatabase StoreConnection { get; }
        public int Port=> int.Parse(config["InventoryLogicServicePort"]);


        public IntegrationTestFixture() 
        {
            var configDefaults = new Dictionary<string, string>
            {
                {"ConnectionStrings:RedisConnection", "localhost:6479,allowAdmin=false"},
                {"InventoryLogicServicePort", "54105"}
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
            redis.Dispose();
        }
    }
}
