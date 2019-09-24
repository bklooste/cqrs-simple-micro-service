
using System;
using System.Collections.Generic;
using EventStore.ClientAPI;

namespace SimpleCQRS.API
{
    public class ExternalLogic
    {
        readonly IEventStoreConnection connection;

        public ExternalLogic(IEventStoreConnection connection)
        {
            this.connection = connection;
        }

        public float GetPrice()
        {
            if ( connection == null)
                return 0.0f;

            return 0.2f;
        }
    }

}
