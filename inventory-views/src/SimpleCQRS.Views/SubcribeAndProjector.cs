using System;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using EventStore.ClientAPI;

using Newtonsoft.Json;

namespace SimpleCQRS.Views
{
    public class SubcribeAndProjector
    {
        const string CategoryStreamName = "$ce-inventory";

        readonly InventoryListView inventoryListView;
        readonly InventoryItemDetailView inventoryView;
        readonly Microsoft.Extensions.Logging.ILogger logger;

        IHostApplicationLifetime appLifeTime;
        EventStoreStreamCatchUpSubscription subscriber;

        public SubcribeAndProjector(InventoryListView inventoryListView, InventoryItemDetailView inventoryView, Microsoft.Extensions.Logging.ILogger logger)
        {
            this.inventoryListView = inventoryListView;
            this.inventoryView = inventoryView;
            this.logger = logger;
        }

        public void ConfigureAndStart(IEventStoreConnection connection, IHostApplicationLifetime applicationLifeTime)
        {
            this.subscriber = connection.SubscribeToStreamFrom(CategoryStreamName, StreamPosition.Start, CatchUpSubscriptionSettings.Default, Project, null, SubscriptionDropped);

            this.appLifeTime = applicationLifeTime;
        }

        void SubscriptionDropped(EventStoreCatchUpSubscription arg1, SubscriptionDropReason reason, Exception ex)
        {
            logger.LogError(ex, $"subscription died restarting reason {reason}");
            appLifeTime.StopApplication();
        }

        Task Project(EventStoreCatchUpSubscription subscription, ResolvedEvent resolvedEvent)
        {
            try
            {
                dynamic evnt = ToEvent(resolvedEvent);
                //if you have many or do IO can do in parallel or asyc
                inventoryListView.Handle(evnt);
                inventoryView.Handle(evnt);
            }
            catch (Exception ex)
            {
                //If you dont ignore your systems stops, processing is best effort and unlike commands cannot be rejected
                logger.LogWarning(ex, $"Ignored message {resolvedEvent.Event.EventType} after best effort processing");
            }

            return Task.CompletedTask;
        }

        static Event ToEvent(ResolvedEvent storeEvent)
        {
            var type = Type.GetType(storeEvent.Event.EventType);
            var json = Encoding.UTF8.GetString(storeEvent.Event.Data);
            return (Event)JsonConvert.DeserializeObject(json, type);
        }
    }
}