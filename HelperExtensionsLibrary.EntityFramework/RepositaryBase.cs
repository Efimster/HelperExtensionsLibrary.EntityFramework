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
        /// <summary>
        /// Entity set
        /// </summary>
        public IDbSet<T> DbSet { get; private set; }
        /// <summary>
        /// Database context
        /// </summary>
        public IObjectContextAdapter DbContext { get; private set; }
        /// <summary>
        /// Table name
        /// </summary>
        protected Lazy<string> TableName
        {
            get;
            set;
        }

        public RepositaryBase(IObjectContextAdapter context, IDbSet<T> dbSet)
        {
            DbSet = dbSet;
            DbContext = context;
            TableName = new Lazy<string>(() => EFExtensions.GetTableName<T>(DbContext), false);
        }
        /// <summary>
        /// Get list of entities according to predicate
        /// </summary>
        /// <param name="predicate">filtering predicate</param>
        /// <returns>list of entities</returns>
        public IList<T> GetAll(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            return DbSet.Where(predicate).ToList();
        }
        /// <summary>
        /// Get entity by predicate
        /// </summary>
        /// <param name="predicate">filtering predicate</param>
        /// <param name="single">true- repositary should contain at most one suited entity </param>
        /// <returns>entity</returns>
        public T GetOne(System.Linq.Expressions.Expression<Func<T, bool>> predicate, bool single = true)
        {
            if (single)
                return DbSet.SingleOrDefault(predicate);
            else
                return DbSet.FirstOrDefault(predicate);
        }
        /// <summary>
        /// Add entity to repositary
        /// </summary>
        /// <param name="one">entity</param>
        /// <param name="autoupdate">commit changes</param>
        public virtual void AddOne(T one, bool autoupdate = true)
        {
            DbSet.Add(one);
            if (autoupdate)
                UpdateAll();
        }
        /// <summary>
        /// Commit changes
        /// </summary>
        public virtual void UpdateAll()
        {
            DbContext.ObjectContext.SaveChanges();
        }
        /// <summary>
        ///  Remove  entity
        /// </summary>
        /// <param name="one">Entity</param>
        /// <param name="autoupdate">Commit changes</param>
        public virtual void DeleteOne(T one, bool autoupdate = true)
        {
            DbSet.Remove(one);
            if (autoupdate)
                UpdateAll();
        }
        /// <summary>
        /// Dispose repositary
        /// </summary>
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
        /// <summary>
        /// Finalizer
        /// </summary>
        ~RepositaryBase()
        {
            Dispose(false);
        }
        
        /// <summary>
        /// Get list of entities according to filter string (select * from TableName where 'filterstring'). Note: DbContext shouold be DbContext type.
        /// </summary>
        /// <param name="filterStr">filtering string</param>
        /// <returns>list of entities</returns>
        public IList<T> GetAll(string filterStr)
        {
            var dbSet = DbSet as DbSet<T>;
            if (dbSet == null)
                return null;

            return dbSet.SqlFilterQuery(TableName.Value, filterStr).ToList();
        }

        /// <summary>
        /// Remove entities range
        /// </summary>
        /// <param name="range">range of entities</param>
        /// <param name="autoupdate">Commit changes</param>
        public void DeleteRange(IEnumerable<T> range, bool autoupdate = true)
        {
            foreach (var entity in range)
                DeleteOne(entity, autoupdate: false);

            if (autoupdate)
                UpdateAll();
        }
        /// <summary>
        /// Get list of all entities 
        /// </summary>
        /// <returns>list of entities</returns>
        public IList<T> GetAll()
        {
            return GetAll(entity => true);
        }

        /// <summary>
        /// Attach entity to context
        /// </summary>
        /// <param name="one">entity</param>
        ///<returns>entity</returns>
        public T AttachOne(T one)
        {
            return DbSet.Attach(one);
        }
    }
}
