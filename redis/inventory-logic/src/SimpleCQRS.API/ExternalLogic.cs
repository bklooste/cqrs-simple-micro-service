
using StackExchange.Redis;
using System;
using System.Collections.Generic;


namespace SimpleCQRS.API
{
    public class ExternalLogic
    {
        readonly IDatabase connection;

        public ExternalLogic(IDatabase connection)
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
