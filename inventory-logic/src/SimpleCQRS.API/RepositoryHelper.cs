using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EventStore.ClientAPI;
using Newtonsoft.Json;


namespace SimpleCQRS.API
{
    public static class RepositoryHelper
    {
        const string CategoryStreamPrefix = "inventory-";

        public static async Task Save(this IEventStoreConnection connection, AggregateRoot aggregate, int expectedVersion)
        {
            var streamName = GetStreamName(aggregate.GetType().Name, aggregate.Id);
            var storeEvents = aggregate.GetUncommittedChanges().Select(x => x.ToStoreEvent());
            await connection.AppendToStreamAsync(streamName, expectedVersion, storeEvents);
        }

        static string GetStreamName(string name, Guid id)
        {

            return CategoryStreamPrefix + name + id.ToString();
        }

        public static async Task<T> GetById<T>(this IEventStoreConnection connection, Guid id) where T : AggregateRoot, new()
        {
            var streamName = GetStreamName(typeof(T).Name, id);

            var events = await connection.ReadStreamEventsForwardAsync(streamName, 0, 4000, false);
            if (events.Status == SliceReadStatus.StreamNotFound)
                throw new AggregateNotFoundException($"id {id} does not exist");

            var obj = new T();//lots of ways to do this
            obj.LoadsFromHistory(events.Events.Select(x => x.ToEvent()));
            return obj;
        }

        public static Event ToEvent(this ResolvedEvent storeEvent)
        {
            var type = Type.GetType(storeEvent.Event.EventType);
            var json = Encoding.UTF8.GetString(storeEvent.Event.Data);
            return (Event)JsonConvert.DeserializeObject(json, type);
        }

        public static EventData ToStoreEvent(this Event evnt)
        {
            var json = JsonConvert.SerializeObject(evnt);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            return new EventData(Guid.NewGuid(), evnt.GetType().FullName, true, jsonBytes, null);
        }

    }

}
