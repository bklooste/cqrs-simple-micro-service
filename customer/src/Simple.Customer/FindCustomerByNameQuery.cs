using System;
using System.Linq;
using System.Linq.Expressions;

using Marten;
using Marten.Linq;

namespace Simple.Customers
{
    //TODO pre compiled queries rock note this returns json for straight out but for more complex logic you can return IEnumerable<Customer>
    // It is NOT needed for small service at all
    public class FindCustomerJsonByNameQuery : ICompiledQuery<Customer,string>
    {
        public string LastNamePrefix { get; set; } = string.Empty;

        public Expression<Func<IQueryable<Customer>, string>> QueryIs()
        {
            return q => q
                .Where(p => p.LastName.StartsWith(LastNamePrefix))
                .OrderBy( p=> p.LastName)
                .ToJsonArray();
        }
    }  
}