using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using HelperExtensionsLibrary.Objects;

namespace HelperExtensionsLibrary.EntityFramework.Testing
{
    /// <summary>
    /// In-memory test repository (storage)
    /// </summary>
    /// <typeparam name="T">Type of storage entities</typeparam>
    public partial class TestRepository<T> : ITestRepository<T>, IEnumerable<T>
           where T : class
    {
        /// <summary>
        /// In-memory data storage
        /// </summary>
        private static TestRepositaryStorage Storage { get; set; }
        /// <summary>
        /// Working data set
        /// </summary>
        public  IList<Item> DbSet { get; private set; }
        /// <summary>
        /// List of attached entities
        /// </summary>
        private IList<T> AttachedList { get; set; }

        public TestRepository()
        {
            if (Storage == null)
                Storage = new TestRepositaryStorage();

            DbSet = Storage.GetSnapshot(this);
            AttachedList = new List<T>();
        }
        /// <summary>
        /// Get list of entities according to predicate
        /// </summary>
        /// <param name="predicate">filtering predicate</param>
        /// <returns>list of entities</returns>
        public IList<T> GetAll(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            return AsEnumerable(predicate).ToList();
        }
        /// <summary>
        /// Get entity by predicate
        /// </summary>
        /// <param name="predicate">filtering predicate</param>
        /// <param name="single">true- repository should contain at most one suited entity </param>
        /// <returns>entity</returns>
        public T GetOne(System.Linq.Expressions.Expression<Func<T, bool>> predicate, bool single = true)
        {
            T one = single ?
                    DbSet.Where(item => item.State.IsNotIn(ItemState.Removed))
                        .Select(item => item.Value).SingleOrDefault(predicate.Compile())
                    : DbSet.Where(item => item.State.IsNotIn(ItemState.Removed))
                        .Select(item => item.Value).FirstOrDefault(predicate.Compile());

            return one;

        }
        /// <summary>
        /// Add entity to repository
        /// </summary>
        /// <param name="one">entity</param>
        /// <param name="autoupdate">commit changes</param>
        public IRepository<T> AddOne(T one, bool autoupdate = true)
        {
            var item = new Item { Value = one, State = ItemState.Added };
            item.TriggerForeignKeyActons();

            DbSet.Add(item);

            if (autoupdate)
                UpdateAll();

            return this;
        }
        /// <summary>
        /// Commit changes
        /// </summary>
        /// <returns>affected rows count</returns>
        public int UpdateAll()
        {
            AttachedList.Clear();
            return Storage.Commit(this, DbSet);
        }
        /// <summary>
        ///  Remove  entity
        /// </summary>
        /// <param name="one">Entity</param>
        /// <param name="autoupdate">Commit changes</param>
        /// <param name="attache">true:attache entity. Default false.</param>
        public IRepository<T> DeleteOne(T one, bool autoupdate = true, bool attache = false)
        {
            if (attache)
                AttachOne(one);

            var oneItem = new Item() { Value = one };

            var item = DbSet.FirstOrDefault(x => x.State.IsNotIn(ItemState.Removed) && x.Equals(oneItem));
            if (item != null)
            {
                if (item.State.IsIn(ItemState.Added))
                {
                    DbSet.Remove(item);
                    return this;
                }

                if (!(item.Value.Equals(one) || AttachedList.Contains(one)))
                    throw new InvalidOperationException("item doesn't attached to context");

                item.State = ItemState.Removed;
            }

            if (autoupdate)
                UpdateAll();

            return this;
        }
        /// <summary>
        /// Disposes repository object
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed && IsDisposedInternal)
                return;

            IsDisposed = true;

            //if (currentTransaction != null)
            //    return;

            IsDisposedInternal = true;
            
            DbSet = null;
            if (AttachedList != null)
            {
                AttachedList.Clear();
                AttachedList = null;
            }
        }
        /// <summary>
        /// Clears working data set
        /// </summary>
        public void Clear()
        {
            DbSet.Clear();
        }
        /// <summary>
        /// Get list of entities according to predicate
        /// </summary>
        /// <param name="filterString">filter string</param>
        /// <returns>list of entities</returns>
        public IList<T> GetAll(string filterString)
        {
            return BuildQueryByFilterString(filterString).ToList();
        }
        /// <summary>
        /// Builds query by filter string
        /// </summary>
        /// <param name="filterString"></param>
        /// <returns></returns>
        private IQueryable<T> BuildQueryByFilterString(string filterString)
        {
            //var dict = filterString.SplitToDictionaryExt("and", "=");

            var builder = QueryBuilder.Parse(filterString);
            
            IQueryable<T> query = DbSet.Where(entry =>
            {
                var entryProperties = entry.Value.ToPropertyValuesDictionary(filterDefaultValues: false);
                foreach (var filter in builder.Queries)
                {
                    //if (!entryProperties.Any(prop => prop.Key == filter.Key && prop.Value.ToString() == filter.Value.Trim('\'')))
                    if (!filter.Filter(entryProperties))
                        return false;
                }

                return true;

            }).Select(entry => entry.Value).AsQueryable();

            return query;
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
        /// Removes entities filtered by predicate
        /// </summary>
        /// <param name="predicate">filtering predicate</param>
        /// <param name="autoupdate">commit changes</param>
        /// <returns>repository</returns>
        public IRepository<T> DeleteRange(System.Linq.Expressions.Expression<Func<T, bool>> predicate, bool autoupdate = false)
        {
            return DeleteRange(DbSet.Select(item => item.Value).Where(predicate.Compile()), autoupdate: autoupdate, attache: false);
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
            if (!IsAttached(one))
                AttachedList.Add(one);

            return one;

        }
        /// <summary>
        /// Checks whether entity is attached to context 
        /// </summary>
        /// <param name="entity">entity</param>
        /// <returns>true: attached, false: not attached</returns>
        public bool IsAttached(T entity)
        {
            var oneItem = DbSet.FirstOrDefault(item => item.Equals(entity));
            if (oneItem != null)
                return true;

            var one = AttachedList.FirstOrDefault(item => item.Equals(entity));
            if (one != null)
                return true;

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
        public bool IsDisposed { get; set; }
        protected bool IsDisposedInternal { get; set; }
        /// <summary>
        /// Makes repository queryable
        /// </summary>
        /// <returns>queryable interface</returns>
        public IQueryable<T> AsQueryable()
        {
            return DbSet.AsQueryable().Where(item => item.State.IsNotIn(ItemState.Removed)).Select(item => item.Value);
        }
        /// <summary>
        /// Makes repository queryable
        /// </summary>
        /// <param name="filterString">filtering string</param>
        /// <returns>queryable interface</returns>
        public IQueryable<T> AsQueryable(string filterString)
        {
            return BuildQueryByFilterString(filterString);
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
            return DbSet.Where(item => item.State.IsNotIn(ItemState.Removed))
                .Select(item => item.Value)
                .Where(predicate.Compile());
        }
        /// <summary>
        /// Enumerates repository items filtered by filter string
        /// </summary>
        /// <param name="filterString">filtering string</param>
        /// <returns>enumerable list</returns>
        public IEnumerable<T> AsEnumerable(string filterString)
        {
            return BuildQueryByFilterString(filterString);
        }
        /// <summary>
        /// Enumerates repository items
        /// </summary>
        /// <returns>enumerable list</returns>
        public IEnumerable<T> AsEnumerable()
        {
            return AsEnumerable(x=>true);
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
            Action<T,TProperty> setter;

            var foreignKeyPredicate = TestRepositoryHelper.GetForeinKeyPredicate(includePath, out setter).Compile();

            var includeQuery = AsEnumerable()
                .Where(predicate.Compile())
                .Select(item =>
            {
                var include = foreignSource.FirstOrDefault(foreign =>
                {
                    return foreignKeyPredicate(item, foreign);
                });

                if (include != null)
                    setter(item, include);

                return item;
            });

            return includeQuery.AsQueryable();
        }
        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns> Enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return AsEnumerable().GetEnumerator();
        }
        /// <summary>
        ///  Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>Enumerator that can be used to iterate through the collection.</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    [Flags]
    public enum ItemState
    {
        None = 0,
        Unchanged = 1,
        Added = 2,
        Removed = 4,
        Changed = 8,

    }
}
