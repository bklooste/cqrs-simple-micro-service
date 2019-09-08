using System;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using EventStore.ClientAPI;

using Newtonsoft.Json;

namespace SimpleCQRS.Views
{
    // read events isolating our dependencies eg Eventstore and we can test Poco code
    public class EventSubscriber
    {
        const string CategoryStreamName = "$ce-inventory";
        const int TwoMinutesMaxStartTime = 120;
        
        readonly Microsoft.Extensions.Logging.ILogger logger;
        readonly Func<Event,Task> playEvent;
        readonly IHostApplicationLifetime appLifeTime;
        readonly IEventStoreConnection connection;

        EventStoreStreamCatchUpSubscription? subscriber;

        public EventSubscriber(IEventStoreConnection connection, Func<Event, Task> playEvent, IHostApplicationLifetime applicationLifeTime, Microsoft.Extensions.Logging.ILogger logger)
        {
            this.logger = logger;
            this.appLifeTime = applicationLifeTime;
            this.playEvent = playEvent;
            this.connection = connection;
        }

        public void Start()
        {
            if (subscriber != null)
                logger.LogError("Subscriber already started");

            Task convertAndPlayEvent(EventStoreCatchUpSubscription sub, ResolvedEvent storeEvent) => playEvent(ToEvent(storeEvent));
            void logStart(EventStoreCatchUpSubscription sub) => LiveProcessingStarted(sub, DateTime.Now);
            this.subscriber = connection.SubscribeToStreamFrom(CategoryStreamName, StreamPosition.Start, CatchUpSubscriptionSettings.Default, convertAndPlayEvent, logStart, SubscriptionDropped);
        }

        void LiveProcessingStarted(EventStoreCatchUpSubscription sub, DateTime timeStarted)
        {
            var timeToStart = (DateTime.UtcNow - timeStarted).Seconds;
            if (timeToStart > TwoMinutesMaxStartTime)
                logger.LogWarning($"EventStore Subscription live processing started , loading took {timeToStart} seconds. {sub.SubscriptionName} Is it time to redesign views");
            else
                logger.LogInformation($"EventStore Subscription live processing started , loading took {timeToStart} seconds");
        }

        void SubscriptionDropped(EventStoreCatchUpSubscription sub, SubscriptionDropReason reason, Exception ex)
        {
            if (ex != null)
                logger.LogWarning(ex, $"EventStore Subscription Dropped {reason.ToString()}, {sub.SubscriptionName} restarting service");
            else
                logger.LogWarning($"EventStore Subscription Dropped, {reason.ToString()} restarting service");
            appLifeTime.StopApplication();
        }

        static Event ToEvent(ResolvedEvent storeEvent)
        {
            var type = Type.GetType(storeEvent.Event.EventType);
            var json = Encoding.UTF8.GetString(storeEvent.Event.Data);
            return (Event)JsonConvert.DeserializeObject(json, type);
        }
    }
}