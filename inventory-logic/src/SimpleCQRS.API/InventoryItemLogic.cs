
using System;
using System.Diagnostics;

namespace SimpleCQRS.API
{
    public partial class InventoryItemLogic : AggregateRoot
    {
        bool activated;
        Guid id;

        public InventoryItemLogic() { }

        public InventoryItemLogic(Guid id, string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name is not valid");
            ApplyChange(new InventoryItemCreated(id, name));
        }

        public void ChangeName(string newName)
        {
            if (string.IsNullOrEmpty(newName))
                throw new ArgumentException("newName is not valid");
            ApplyChange(new InventoryItemRenamed(id, newName));
        }

        public void Remove(int count)
        {
            if (count <= 0)
                throw new InvalidOperationException("cant remove negative count from inventory");
            ApplyChange(new ItemsRemovedFromInventory(id, count));
        }

        public void CheckIn(int count)
        {
            if (count <= 0)
                throw new InvalidOperationException("must have a count greater than 0 to add to inventory");
            ApplyChange(new ItemsCheckedInToInventory(id, count));
        }

        public void Deactivate()
        {
            if (!activated)
                throw new InvalidOperationException("already deactivated");
            ApplyChange(new InventoryItemDeactivated(id));
        }

        public override Guid Id
        {
            get { return id; }
        }

        // Applies can go to partial class when you have a lot of logic , fix internal via other way at home
        internal void Apply(InventoryItemCreated e)
        {
            id = e.Id;
            activated = true;
        }

        internal void Apply(InventoryItemDeactivated e)
        {
            activated = false;
        }

        internal void Apply(Event e)
        {
            Debug.WriteLine($"nothing to play for event {e.GetType().Name} - dont delete this method");
        }
    }
}
