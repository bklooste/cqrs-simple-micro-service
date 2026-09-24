using System;

using RedisEvents.EventSourcing;
using RedisEvents.Projections;

using SimpleCQRS;

namespace SimpleCQRS.Views.IntegrationTest
{
    // A minimal stand-in for the command side's InventoryItemLogic, so these tests can publish a
    // real InventoryItemCreated event - same topic, same aggregate name, same wire type - without
    // depending on the command-side service assembly.
    internal sealed class TestInventoryItem : AggregateRoot
    {
        Guid id;

        public TestInventoryItem()
        {
            On<InventoryItemCreated>(e => id = e.Id);
        }

        public override string AggregateName => "Inventory";

        public override string Id => id.ToString();

        public static TestInventoryItem Create(Guid id, string name)
        {
            var item = new TestInventoryItem();
            item.Raise(new InventoryItemCreated(id, name));
            return item;
        }
    }
}
