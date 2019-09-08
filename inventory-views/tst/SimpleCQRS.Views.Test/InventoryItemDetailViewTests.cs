using System;

using Xunit;
using AutoFixture.Xunit2;

using SimpleCQRS.Views;
using System.Collections.Generic;

namespace SimpleCQRS.API.Test
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Long test names")]

    public class InventoryItemDetailViewTests
    {
        [Theory, AutoData]
        public void when_created_then_create_message_correct(InventoryItemCreated msg)
        {
            var view = new InventoryItemDetailView();

            view.Handle(msg);

            var item = view.Repository[msg.Id];
            Assert.Equal(msg.Id, item.Id);
            Assert.Equal(msg.Name, item.Name);
            Assert.Equal(0, item.CurrentCount);
        }

        [Theory, AutoData]
        public void given_existing_item_when_receive_items_checked_in_then_expected_count_correct(ItemsCheckedInToInventory msg, int preCount, string name)
        {
            var repository = new Dictionary<Guid, InventoryItemDetailsDto>() { { msg.Id, new InventoryItemDetailsDto(msg.Id, name, preCount, 0)  }  };
            var view = new InventoryItemDetailView(repository);

            view.Handle(msg);

            var item = view.Repository[msg.Id];
            Assert.Equal(msg.Id, item.Id);
            var expectedCount = preCount + msg.Count;
            Assert.Equal(expectedCount, item.CurrentCount);
        }
    }
}

