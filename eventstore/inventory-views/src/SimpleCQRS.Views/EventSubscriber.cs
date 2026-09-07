using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using EventStore.Client;

using Newtonsoft.Json;

namespace SimpleCQRS.Views
{
    // read events isolating our dependencies eg Eventstore and we can test Poco code
    public class EventSubscriber
    {
        const string CategoryStreamName = "$ce-inventory";

        readonly Microsoft.Extensions.Logging.ILogger logger;
        readonly Func<Event,Task> playEvent;
        readonly IHostApplicationLifetime appLifeTime;
        readonly EventStoreClient connection;

        StreamSubscription? subscriber;

        public EventSubscriber(EventStoreClient connection, Func<Event, Task> playEvent, IHostApplicationLifetime applicationLifeTime, Microsoft.Extensions.Logging.ILogger logger)
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

            Task convertAndPlayEvent(StreamSubscription sub, ResolvedEvent storeEvent, CancellationToken ct) => playEvent(ToEvent(storeEvent));

            this.subscriber = connection
                .SubscribeToStreamAsync(CategoryStreamName, FromStream.Start, convertAndPlayEvent, resolveLinkTos: true, subscriptionDropped: SubscriptionDropped)
                .GetAwaiter().GetResult();

            logger.LogInformation($"EventStore Subscription live processing started, {subscriber.SubscriptionId}");
        }

        void SubscriptionDropped(StreamSubscription sub, SubscriptionDroppedReason reason, Exception ex)
        {
            if (ex != null)
                logger.LogWarning(ex, $"EventStore Subscription Dropped {reason.ToString()}, {sub.SubscriptionId} restarting service");
            else
                logger.LogWarning($"EventStore Subscription Dropped, {reason.ToString()} restarting service");
            appLifeTime.StopApplication();
        }

        static Event ToEvent(ResolvedEvent storeEvent)
        {
            var type = Type.GetType(storeEvent.Event.EventType);
            var json = Encoding.UTF8.GetString(storeEvent.Event.Data.Span);
            return (Event)JsonConvert.DeserializeObject(json, type);
        }
    }
}
