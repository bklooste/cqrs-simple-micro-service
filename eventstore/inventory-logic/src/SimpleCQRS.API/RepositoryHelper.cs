using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EventStore.Client;
using Newtonsoft.Json;


namespace SimpleCQRS.API
{
    public static class ExpectedVersion
    {
        public const int NoStream = -1;
        public const int Any = -2;
    }

    public static class RepositoryHelper
    {
        const string CategoryStreamPrefix = "inventory-";

        static string GetStreamName(string name, Guid id)
        {
            return CategoryStreamPrefix + name + id.ToString();
        }

        public static async Task Save(this EventStoreClient connection, AggregateRoot aggregate, int expectedVersion)
        {
            var streamName = GetStreamName(aggregate.GetType().Name, aggregate.Id);
            var storeEvents = aggregate.GetUncommittedChanges().Select(x => x.ToStoreEvent());

            if (expectedVersion == ExpectedVersion.NoStream)
                await connection.AppendToStreamAsync(streamName, StreamState.NoStream, storeEvents);
            else if (expectedVersion == ExpectedVersion.Any)
                await connection.AppendToStreamAsync(streamName, StreamState.Any, storeEvents);
            else
                await connection.AppendToStreamAsync(streamName, StreamRevision.FromInt64(expectedVersion), storeEvents);

            aggregate.MarkChangesAsCommitted();
        }

        public static async Task<T> GetById<T>(this EventStoreClient connection, Guid id) where T : AggregateRoot, new()
        {
            var streamName = GetStreamName(typeof(T).Name, id);

            var result = connection.ReadStreamAsync(Direction.Forwards, streamName, StreamPosition.Start, 4000, resolveLinkTos: false);
            if (await result.ReadState == ReadState.StreamNotFound)
                throw new AggregateNotFoundException($"id {id} does not exist");

            var events = new List<Event>();
            await foreach (var storeEvent in result)
                events.Add(storeEvent.ToEvent());

            var obj = new T();//lots of ways to do this
            obj.LoadsFromHistory(events);
            return obj;
        }

        public static Event ToEvent(this ResolvedEvent storeEvent)
        {
            var type = Type.GetType(storeEvent.Event.EventType);
            var json = Encoding.UTF8.GetString(storeEvent.Event.Data.Span);
            return (Event)JsonConvert.DeserializeObject(json, type);
        }

        public static EventData ToStoreEvent(this Event evnt)
        {
            var json = JsonConvert.SerializeObject(evnt);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            return new EventData(Uuid.NewUuid(), evnt.GetType().FullName, jsonBytes);
        }

    }

}
