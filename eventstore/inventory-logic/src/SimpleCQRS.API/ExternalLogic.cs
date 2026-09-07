
using System;
using System.Collections.Generic;
using EventStore.Client;

namespace SimpleCQRS.API
{
    public class ExternalLogic
    {
        readonly EventStoreClient connection;

        public ExternalLogic(EventStoreClient connection)
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
