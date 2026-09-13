using RedisEvents.EventSourcing;

using SimpleCQRS;

namespace SimpleCQRS.Views
{
    // Registers the wire type for each inventory event. Passed as the Action<EventTypeRegistry>
    // that AddEventStore and AddEventProjector both take, so the command side and the read side
    // always agree on how events are (de)serialised.
    public static class InventoryEventTypes
    {
        public static void Register(EventTypeRegistry events)
        {
            events.RegisterJson("inventory.created", InventoryJsonContext.Default.InventoryItemCreated)
                  .RegisterJson("inventory.renamed", InventoryJsonContext.Default.InventoryItemRenamed)
                  .RegisterJson("inventory.checked-in", InventoryJsonContext.Default.ItemsCheckedInToInventory)
                  .RegisterJson("inventory.removed", InventoryJsonContext.Default.ItemsRemovedFromInventory)
                  .RegisterJson("inventory.deactivated", InventoryJsonContext.Default.InventoryItemDeactivated);
        }
    }
}
