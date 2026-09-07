using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Marten;
using Marten.Linq;

namespace Simple.Customers
{
    //TODO pre compiled queries rock, for small services this level of ceremony isn't strictly needed
    public class FindCustomerJsonByNameQuery : ICompiledListQuery<Customer>
    {
        public string LastNamePrefix { get; set; } = string.Empty;

        public Expression<Func<IMartenQueryable<Customer>, IEnumerable<Customer>>> QueryIs()
        {
            return q => q
                .Where(p => p.LastName.StartsWith(LastNamePrefix))
                .OrderBy( p=> p.LastName);
        }
    }
}
