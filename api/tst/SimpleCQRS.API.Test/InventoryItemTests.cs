using System;
using System.Linq;

using AutoFixture.Xunit2;
using Xunit;

namespace SimpleCQRS.API.Test
{
    // pure black test methods with no dependencies
    // logic can be well tested and when changing only actual logic that fails will change
    // coverage will be extremely high with all wiring comming from integration tests
    public class InventoryItemTests
    {
        [Theory, AutoData]
        public void when_created_then_create_message_correct(Guid id, string name)
        {
            var itemBL = new InventoryItemLogic(id, name);

            var events = itemBL.GetUncommittedChanges();
            Assert.Single(events);
            Assert.IsType<InventoryItemCreated>(events.First());
            var createEvent = events.First() as InventoryItemCreated;
            Assert.Equal(id, createEvent.Id);
            Assert.Equal(name, createEvent.Name);
        }

        [Theory, AutoData]
        public void given_created_when_add_then_create_message_correct(Guid id, string name, int count)
        {
            var itemBL = new InventoryItemLogic(id, name);
            itemBL.MarkChangesAsCommitted();

            itemBL.CheckIn(count);

            var events = itemBL.GetUncommittedChanges();
            var checkedInEvent = events.First() as ItemsCheckedInToInventory;
            Assert.Equal(id, checkedInEvent.Id);
            Assert.Equal(count, checkedInEvent.Count);
        }

        [Theory, AutoData]
        public void given_deactivated_when_dectivate_then_fails(Guid id, string name)
        {
            var itemBL = new InventoryItemLogic(id, name);
            itemBL.Deactivate();
            itemBL.MarkChangesAsCommitted();

            Assert.Throws<InvalidOperationException>( () => itemBL.Deactivate());
        }
    }
}
