using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace SimpleCQRS.Views
{
    public class EventProjector
    {
        const string CategoryStreamName = "$ce-inventory";

        readonly InventoryListView inventoryListView;
        readonly InventoryItemDetailView inventoryView;
        readonly Microsoft.Extensions.Logging.ILogger logger;

        public EventProjector(InventoryListView inventoryListView, InventoryItemDetailView inventoryView, ILogger<EventProjector> logger)
        {
            this.inventoryListView = inventoryListView;
            this.inventoryView = inventoryView;
            this.logger = logger;
        }

        public Task ProjectBatch(Event[] resolvedEvent)
        {
            foreach (var evnt in resolvedEvent)
                Project(evnt);

            return Task.CompletedTask;
        }

        public Task Project(Event resolvedEvent)
        {
            try
            {
                //if you have many or do IO can do in parallel or asyc
                inventoryListView.Handle((dynamic) resolvedEvent);
                inventoryView.Handle((dynamic) resolvedEvent);
            }
            catch (Exception ex)
            {
                //If you dont ignore your systems stops, processing is best effort and unlike commands cannot be rejected
                logger.LogWarning(ex, $"Ignored message {resolvedEvent.GetType().Name} after best effort processing");
            }
            return Task.CompletedTask;
        }
    }
}