
using System;
using System.Collections.Generic;

namespace Simple.Customers
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string LastName { get; set; } = "No LastName";
        public string FirstName { get; set; } = "No FirstName";
    }
}