using System;

namespace SimpleCQRS
{
    // Do not mutate these
    public class InventoryItemCreated
    {
        public readonly Guid Id;
        public readonly string Name;
        public InventoryItemCreated(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    public class InventoryItemDeactivated
    {
        public readonly Guid Id;

        public InventoryItemDeactivated(Guid id)
        {
            Id = id;
        }
    }

    public class InventoryItemRenamed
    {
        public readonly Guid Id;
        public readonly string NewName;

        public InventoryItemRenamed(Guid id, string newName)
        {
            Id = id;
            NewName = newName;
        }
    }

    public class ItemsCheckedInToInventory
    {
        public Guid Id;
        public readonly int Count;

        public ItemsCheckedInToInventory(Guid id, int count)
        {
            Id = id;
            Count = count;
        }
    }

    public class ItemsRemovedFromInventory
    {
        public Guid Id;
        public readonly int Count;

        public ItemsRemovedFromInventory(Guid id, int count)
        {
            Id = id;
            Count = count;
        }
    }
}
