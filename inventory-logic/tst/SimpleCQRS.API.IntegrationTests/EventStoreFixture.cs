using EventStore.ClientAPI;
using System;


namespace SimpleCQRS.API.IntegrationTest
{
    public class EventStoreFixture : IDisposable
    {
        public readonly IEventStoreConnection StoreConnection;

        public EventStoreFixture() 
        {
            StoreConnection = EventStoreConnection.Create("ConnectTo=tcp://admin:changeit@127.0.0.1:1113", "integrationTests");
            StoreConnection.ConnectAsync().Wait();
        }

        public void Dispose()
        {
            StoreConnection.Dispose();
        }
    }
}
