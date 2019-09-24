using Marten.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Simple.Customers
{
    //Linq is super expensive this helps
    public class FindCustomerByNameQuery : ICompiledListQuery<Customer>
    {
        public string LastNamePrefix { get; set; } = string.Empty;

        public Expression<Func<IQueryable<Customer>, IEnumerable<Customer>>> QueryIs()
        {
            return q => q.Where(p => p.LastName.StartsWith(LastNamePrefix));
        }
    }
}