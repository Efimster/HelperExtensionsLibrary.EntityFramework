using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperExtensionsLibrary.EntityFramework
{
    public class GeneralRepository<TUser, TContext> : RepositoryBase<TUser>
        where TUser : class
        where TContext : DbContext
    {
        public GeneralRepository(TContext context)
            : base(context, context.Set<TUser>())
        {

        }
    }
}
