using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace HelperExtensionsLibrary.EntityFramework
{
    /// <summary>
    /// Abstract IRepositary implementation
    /// </summary>
    /// <typeparam name="T">Type of repositary items</typeparam>
    public abstract class RepositaryBase<T> : IRepositary<T>
        where T : class
    {
        public IDbSet<T> DbSet { get; private set; }
        public IObjectContextAdapter DbContext { get; private set; }

        public RepositaryBase(IObjectContextAdapter context, IDbSet<T> dbSet)
        {
            DbSet = dbSet;
        }

        public IList<T> GetAll(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            return DbSet.Where(predicate).ToList();
        }

        public T GetOne(System.Linq.Expressions.Expression<Func<T, bool>> predicate, bool single = true)
        {
            if (single)
                return DbSet.SingleOrDefault(predicate);
            else
                return DbSet.FirstOrDefault(predicate);
        }

        public virtual void AddOne(T one, bool autoupdate = true)
        {
            DbSet.Add(one);
        }

        public virtual void UpdateAll()
        {
            DbContext.ObjectContext.SaveChanges();
        }


        public virtual void DeleteOne(T one, bool autoupdate = true)
        {
            DbSet.Remove(one);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (dispose)
            {
            }

            var context = DbContext as IDisposable;

            if (context != null)
                context.Dispose();

            DbContext = null;

        }

        ~RepositaryBase()
        {
            Dispose(false);
        }

    }
}
