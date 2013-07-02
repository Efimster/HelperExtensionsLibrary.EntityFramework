using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace HelperExtensionsLibrary.EntityFramework
{
    /// <summary>
    /// Abstract IRepository implementation
    /// </summary>
    /// <typeparam name="T">Type of repository items</typeparam>
    public abstract class RepositoryBase<T> : IRepository<T>, IEnumerable<T>
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

        public RepositoryBase(IObjectContextAdapter context, IDbSet<T> dbSet)
        {
            DbSet = dbSet;
            DbContext = context;
            TableName = new Lazy<string>(() => EFExtensions.GetTableName<T>(DbContext), false);
            IsDisposed = false;
            
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
        /// <param name="single">true- repository should contain at most one suited entity </param>
        /// <returns>entity</returns>
        public T GetOne(System.Linq.Expressions.Expression<Func<T, bool>> predicate, bool single = true)
        {
            if (single)
                return DbSet.SingleOrDefault(predicate);
            else
                return DbSet.FirstOrDefault(predicate);
        }
        /// <summary>
        /// Add entity to repository
        /// </summary>
        /// <param name="one">entity</param>
        /// <param name="autoupdate">commit changes</param>
        public virtual IRepository<T> AddOne(T one, bool autoupdate = true)
        {
            DbSet.Add(one);
            if (autoupdate)
                UpdateAll();

            return this;
        }
        /// <summary>
        /// Commit changes
        /// </summary>
        /// <returns>affected rows count</returns>
        public virtual int UpdateAll()
        {
            return DbContext.ObjectContext.SaveChanges();
        }
        /// <summary>
        ///  Remove  entity
        /// </summary>
        /// <param name="one">Entity</param>
        /// <param name="autoupdate">Commit changes</param>
        /// <param name="attache">true:attache entity. Default false.</param>
        public virtual IRepository<T> DeleteOne(T one, bool autoupdate = true, bool attache = false)
        {
            if (attache && !IsAttached(one))
                AttachOne(one);

            DbSet.Remove(one);
            if (autoupdate)
                UpdateAll();

            return this;
        }
        /// <summary>
        /// Dispose repository
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (IsDisposed)
                return;
            
            IsDisposed = true;
            
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
        ~RepositoryBase()
        {
            Dispose(false);
        }
        
        /// <summary>
        /// Get list of entities according to filter string (select * from TableName where 'filterstring'). 
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
        /// <param name="attache">true:attaches entity. Default false.</param>
        public IRepository<T> DeleteRange(IEnumerable<T> range, bool autoupdate = true, bool attache = false)
        {
            foreach (var entity in range)
                DeleteOne(entity, autoupdate: false, attache: attache);

            if (autoupdate)
                UpdateAll();

            return this;
        }
        /// <summary>
        /// Get list of all entities 
        /// </summary>
        /// <returns>list of entities</returns>
        public IList<T> GetAll()
        {
            return DbSet.ToList();
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
        /// <summary>
        /// Checks whether entity is attached to context 
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns>true: attached, false: not attached</returns>
        public bool IsAttached(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("key");
            }
            ObjectStateEntry entry;
            if (DbContext.ObjectContext.ObjectStateManager.TryGetObjectStateEntry(entity, out entry))
            {
                return (entry.State != EntityState.Detached);
            }
            return false;
        }

        /// <summary>
        /// Add entities range to repository
        /// </summary>
        /// <param name="range">range of entities</param>
        /// <param name="autoupdate">commit changes</param>
        public IRepository<T> AddRange(IEnumerable<T> range, bool autoupdate = true)
        {
            foreach (var entity in range)
                AddOne(entity, autoupdate: false);

            if (autoupdate)
                UpdateAll();

            return this;
        }

        /// <summary>
        /// Indicates whether repository has been disposed
        /// </summary>
        public bool IsDisposed
        {
            get;
            set;
        }

        /// <summary>
        /// Makes repository queryable
        /// </summary>
        /// <returns>queryable interface</returns>
        public IQueryable<T> AsQueryable()
        {
            return DbSet.AsQueryable();
        }

        /// <summary>
        /// Makes repository queryable
        /// </summary>
        /// <param name="filterString">filtering string</param>
        /// <returns>queryable interface</returns>
        public IQueryable<T> AsQueryable(string filterString)
        {
            return AsEnumerable(filterString).AsQueryable();
        }

        /// <summary>
        /// Removes entities filtered by predicate
        /// </summary>
        /// <param name="predicate">filtering predicate</param>
        /// <param name="autoupdate">commit changes</param>
        /// <returns>repository</returns>
        public IRepository<T> DeleteRange(System.Linq.Expressions.Expression<Func<T, bool>> predicate, bool autoupdate = false)
        {
            return DeleteRange(DbSet.Where(predicate), autoupdate: autoupdate, attache: false);
        }

        /// <summary>
        /// Makes repository queryable and filter by predicate
        /// </summary>
        /// <param name="predicate">filtering predicate</param>
        /// <returns>queryable interface</returns>
        public IQueryable<T> AsQueryable(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            return AsEnumerable(predicate).AsQueryable();
        }
        /// <summary>
        /// Enumerates repository items filterd by predicate
        /// </summary>
        /// <param name="predicate">filtering predicate</param>
        /// <returns>enumerable list</returns>
        public IEnumerable<T> AsEnumerable(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            return DbSet.Where(predicate);
        }
        /// <summary>
        /// Enumerates repository items filtered by filter string
        /// </summary>
        /// <param name="filterString">filtering string</param>
        /// <returns>enumerable list</returns>
        public IEnumerable<T> AsEnumerable(string filterString)
        {
            var dbSet = DbSet as DbSet<T>;
            if (dbSet == null)
                return null;

            return dbSet.SqlFilterQuery(TableName.Value, filterString).AsEnumerable();
        }
        /// <summary>
        /// Enumerates repository items
        /// </summary>
        /// <returns>enumerable list</returns>
        public IEnumerable<T> AsEnumerable()
        {
            return DbSet;
        }
        /// <summary>
        /// Filter the sequence of values base on a predicate
        /// and Specifies the related objects to include in the query results
        /// </summary>
        /// <typeparam name="TProperty">entity refernced by foreign key</typeparam>
        /// <param name="predicate">predicate</param>
        /// <param name="includePath">The related object to return in the query results.</param>
        /// <param name="foreignSource">source of foreign type entities</param>
        /// <returns>A new System.Linq.IQueryable<T> with the defined query path.</returns>
        public IQueryable<T> GetAllAndInclude<TProperty>(Expression<Func<T, bool>> predicate, Expression<Func<T, TProperty>> includePath, IEnumerable<TProperty> foreignSource) where TProperty : class
        {
            return DbSet.Where(predicate).Include(includePath);

        }

        public IEnumerator<T> GetEnumerator()
        {
            return AsEnumerable().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
