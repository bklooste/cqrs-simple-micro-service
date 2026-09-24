using System;

using RedisEvents.EventSourcing;
using RedisEvents.Projections;

namespace SimpleCQRS.API
{
    public partial class InventoryItemLogic : AggregateRoot
    {
        bool activated;
        Guid id;

        public InventoryItemLogic()
        {
            On<InventoryItemCreated>(e =>
            {
                id = e.Id;
                activated = true;
            });

            On<InventoryItemDeactivated>(e => activated = false);
            On<InventoryItemRenamed>(e => { });
            On<ItemsCheckedInToInventory>(e => { });
            On<ItemsRemovedFromInventory>(e => { });
        }

        public InventoryItemLogic(Guid id, string name) : this()
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name is not valid");
            Raise(new InventoryItemCreated(id, name));
        }

        public void ChangeName(string newName)
        {
            if (string.IsNullOrEmpty(newName))
                throw new ArgumentException("newName is not valid");
            Raise(new InventoryItemRenamed(id, newName));
        }

        public void Remove(int count)
        {
            if (count <= 0)
                throw new InvalidOperationException("cant remove negative count from inventory");
            Raise(new ItemsRemovedFromInventory(id, count));
        }

        public void CheckIn(int count, float price)
        {
            if (count <= 0)
                throw new InvalidOperationException($"must have a count greater than 0 to add to inventory pirce {price}");
            Raise(new ItemsCheckedInToInventory(id, count));
        }

        public void Deactivate()
        {
            if (!activated)
                throw new InvalidOperationException("already deactivated");
            Raise(new InventoryItemDeactivated(id));
        }

        public override string AggregateName => "Inventory";

        public override string Id => id.ToString();
    }
}
