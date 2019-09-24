using System;
using System.Linq;

using AutoFixture.Xunit2;
using Xunit;

namespace Simple.Customers.Test
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Long test names")]
    // at present there is no logic so everything is an integration test
    public class LogicTests
    {
        //[Theory, AutoData]
        //public void when_created_then_create_message_correct(Guid id, string name)
        //{
        //    var itemBL = new InventoryItemLogic(id, name);

        //    var events = itemBL.GetUncommittedChanges();
        //    Assert.Single(events);
        //    Assert.IsType<InventoryItemCreated>(events.First());
        //    var createEvent = events.First() as InventoryItemCreated;
        //    Assert.Equal(id, createEvent.Id);
        //    Assert.Equal(name, createEvent.Name);
        //}

    }
}
