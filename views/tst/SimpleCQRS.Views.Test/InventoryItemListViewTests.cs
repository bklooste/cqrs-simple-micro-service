using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;
using AutoFixture.Xunit2;

using SimpleCQRS.Views;


namespace SimpleCQRS.API.Test
{

    public class InventoryItemListViewTests
    {
        [Theory, AutoData]
        public void when_created_then_create_message_correct(InventoryItemCreated msg)
        {
            var view = new InventoryListView();

            view.Handle(msg);

            var item = view.Repository.First();
            Assert.Equal(msg.Id, item.Id);
            Assert.Equal(msg.Name, item.Name);
        }
    }
}

