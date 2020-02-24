using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using Newtonsoft.Json;
using StackExchange.Redis;

namespace SimpleCQRS.API
{
    public static class RepositoryHelper
    {
        const string CategoryStreamName = "$ce-inventory";
        const string CategoryStreamPrefix = "inventory-";

        static string GetStreamName(string name, Guid id)
        {
            return CategoryStreamPrefix + name + id.ToString();
        }

        public static async Task Save(this IDatabase connection, AggregateRoot aggregate, int expectedVersion)
        {
            var streamName = GetStreamName(aggregate.GetType().Name, aggregate.Id);

            var tasks = aggregate
                .GetUncommittedChanges()
                .Select(ToRedisEvent)
                .Select(redisEvent => Task.Run( async ()  =>
                {
                    var result = await connection.StreamAddAsync(streamName, redisEvent);
                    await connection.StreamAddAsync(CategoryStreamName, CategoryStreamIndexEntry(streamName, (string) result));
                }))
                .ToArray();
            //// need a category stream.. 

            await Task.WhenAll(tasks);

            aggregate.MarkChangesAsCommitted();
        }

        public static async Task<T> GetById<T>(this IDatabase connection, Guid id) where T : AggregateRoot, new()
        {
            var streamName = GetStreamName(typeof(T).Name, id);

            var events = await connection.StreamReadAsync(streamName, "0-0", 4000);
            if (events.Any() == false)
                throw new AggregateNotFoundException($"id {id} does not exist");

            var obj = new T();//lots of ways to do this
            obj.LoadsFromHistory(events.Select(ToEvent));
            return obj;
        }

        public static Event ToEvent(StreamEntry storeEvent)
        {
            var type = Type.GetType(storeEvent.Values.First(x=> x.Name == "type").Value);
            var json = Encoding.UTF8.GetString( (byte[]) storeEvent.Values.First(x => x.Name == "msg").Value);
            return (Event)JsonConvert.DeserializeObject(json, type);
        }

        public static NameValueEntry[] ToRedisEvent(this Event evnt)
        {
            var json = JsonConvert.SerializeObject(evnt);
            var jsonBytes = Encoding.UTF8.GetBytes(json);
            return new NameValueEntry[]
            { 
                new NameValueEntry("id" , Guid.NewGuid().ToString()),
                new NameValueEntry("type" , evnt.GetType().FullName),
                new NameValueEntry("msg" , jsonBytes), 
                new NameValueEntry("partition" , string.Empty), 
                new NameValueEntry("metadata" , string.Empty)
            };
        }

        public static NameValueEntry[] CategoryStreamIndexEntry(string streamName, string entryKey)
        {
            return new NameValueEntry[]
            {
                new NameValueEntry("stream" , streamName),
                new NameValueEntry("key" , entryKey),
            };
        }
    }

}
