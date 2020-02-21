using System.Linq;

using Xunit;
using AutoFixture.Xunit2;

using SimpleCQRS.Views;


namespace SimpleCQRS.API.Test
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Long test names")]

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

