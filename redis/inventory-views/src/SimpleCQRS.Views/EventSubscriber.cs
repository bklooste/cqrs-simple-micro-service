using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using SimpleCQRS;
using StackExchange.Redis;

namespace SimpleCQRS.Views
{

    // read events isolating our dependencies eg Eventstore and we can test Poco code
    public class EventSubscriber
    {
        const string CategoryStreamName = "$ce-inventory";
        const int TwoMinutesMaxStartTime = 120;
        const int BatchSize = 100;

        const int IntervalToCheckForNewMessagesInMs = 5;

        readonly Microsoft.Extensions.Logging.ILogger logger;
        readonly Func<Event[], Task> playEvents;
        readonly IHostApplicationLifetime appLifeTime;
        readonly Func<IDatabase> connection;
        readonly RedisKey streamName = (RedisKey)CategoryStreamName;

        Task? worker;
        RedisValue nextPosition = StreamPosition.Beginning;
        bool liveProcessing = false;

        public EventSubscriber(Func<IDatabase> connection, Func<Event[], Task> playEvents, IHostApplicationLifetime applicationLifeTime, Microsoft.Extensions.Logging.ILogger logger)
        {
            this.logger = logger;
            this.appLifeTime = applicationLifeTime;
            this.playEvents = playEvents;
            this.connection = connection;
        }

        public async Task DoWork()
        {
            while (true)
            {
                if (appLifeTime.ApplicationStopping.IsCancellationRequested)
                    break;

                try
                {
                    var conn = this.connection();
                    var records = await conn.StreamRangeAsync(streamName, nextPosition, null, BatchSize);
                    if (records.Any())
                    {
                        nextPosition = Increment(records.Last().Id);

                        // TODO use eval , we could do this concurrently as well but we want processing ordered so then need to reorder , its really a job for eval
                        var results = new List<Event>();

                        foreach (var record  in records)
                        {
                            var msgs = await conn.StreamRangeAsync(record.Values.FirstOrDefault(x => x.Name == "stream").Value.ToString(), record.Values.FirstOrDefault(x => x.Name == "key").Value, null, 1);
                            results.Add(ToEvent(msgs.First()));
                        }

                        await playEvents(results.ToArray());
                    }
                    else
                    {
                        if (liveProcessing == false)
                        {
                            liveProcessing = true;
                            LiveProcessingStarted(DateTime.UtcNow);
                        }
                        await Task.Delay(IntervalToCheckForNewMessagesInMs);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "BATCH process ignored error " + e.Message + " pos:" + nextPosition.ToString());
                    await Task.Delay(IntervalToCheckForNewMessagesInMs * 10);
                }
            }
        }

        private RedisValue Increment(RedisValue? position)
        {
            if (position == null || position == StreamPosition.Beginning)
                return StreamPosition.Beginning;

            var key = position.ToString().Split("-");
            int count = int.Parse(key[1]);
            count++;
            return key[0] + "-" + count.ToString();

        }

        public void Start()
        {
            if (worker != null)
                logger.LogError("Subscriber already started");

            this.worker = Task.Run(() => DoWork());
        }

        void LiveProcessingStarted(DateTime timeStarted)
        {
            var timeToStart = (DateTime.UtcNow - timeStarted).Seconds;
            if (timeToStart > TwoMinutesMaxStartTime)
                logger.LogWarning($"Redis Subscription live processing started , loading took {timeToStart} seconds. Is it time to redesign views service");
            else
                logger.LogInformation($"Redis Subscription live processing started , loading took {timeToStart} seconds");
        }

        static Event ToEvent(StreamEntry storeEvent)
        {
            var typeString = storeEvent.Values.First(x => x.Name == "type").Value;
            var type = Type.GetType(typeString) ?? throw new InvalidCastException($"cannot convert message {typeString}");
            var json = Encoding.UTF8.GetString(storeEvent.Values.First(x => x.Name == "msg").Value);
            var evnt = JsonConvert.DeserializeObject(json, type) ?? throw new InvalidCastException($"cannot convert message {typeString}");
            return (Event)evnt;
        }
    }
}