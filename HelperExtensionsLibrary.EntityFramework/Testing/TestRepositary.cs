using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelperExtensionsLibrary.Attributes;
using HelperExtensionsLibrary.Strings;
using HelperExtensionsLibrary.IEnumerable;
using HelperExtensionsLibrary.Objects;

namespace HelperExtensionsLibrary.EntityFramework.Testing
{
    public partial class TestRepositary<T> : ITestRepositary<T>
           where T : class
    {
        private static IList<Item> DbSetInternal { get; set; }
        public IList<Item> DbSet { get; private set; }
        private IList<T> AttachedList { get; set; }


        public TestRepositary()
        {
            if (DbSetInternal == null)
                DbSetInternal = new List<Item>();

            DbSet = DbSetInternal;
            AttachedList = new List<T>();
        }


        /// <summary>
        /// Get list of entities according to predicate
        /// </summary>
        /// <param name="predicate">filtering predicate</param>
        /// <returns>list of entities</returns>
        public IList<T> GetAll(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            var func = predicate.Compile();
            return DbSet.Where(item => item.State.IsNotIn(ItemState.Removed))
                .Select(item => item.Value)
                .Where(func).ToList();
        }
        /// <summary>
        /// Get entity by predicate
        /// </summary>
        /// <param name="predicate">filtering predicate</param>
        /// <param name="single">true- repositary should contain at most one suited entity </param>
        /// <returns>entity</returns>
        public T GetOne(System.Linq.Expressions.Expression<Func<T, bool>> predicate, bool single = true)
        {
            return single ?
                    DbSet.Where(item => item.State.IsNotIn(ItemState.Removed))
                        .Select(item => item.Value).SingleOrDefault(predicate.Compile())
                    : DbSet.Where(item => item.State.IsNotIn(ItemState.Removed))
                        .Select(item => item.Value).FirstOrDefault(predicate.Compile());

        }
        /// <summary>
        /// Add entity to repositary
        /// </summary>
        /// <param name="one">entity</param>
        /// <param name="autoupdate">commit changes</param>
        public void AddOne(T one, bool autoupdate = true)
        {
            var item = new Item { Value = one, State = ItemState.Added };
            item.TriggerForeignKeyActons();
            DbSet.Add(item);

            if (autoupdate)
                UpdateAll();
        }
        /// <summary>
        /// Commit changes
        /// </summary>
        public void UpdateAll()
        {
            for (int i = DbSet.Count - 1; i >= 0; i--)
            {
                var item = DbSet[i];
                if (item.State.IsIn(ItemState.Removed))
                {
                    DbSet.RemoveAt(i);
                    continue;
                }

                if (item.State.IsNotIn(ItemState.Unchanged))
                {
                    item.TriggerDbGeneratedActons();
                    item.State = ItemState.Unchanged;
                }
            }

            AttachedList.Clear();
        }
        /// <summary>
        ///  Remove  entity
        /// </summary>
        /// <param name="one">Entity</param>
        /// <param name="autoupdate">Commit changes</param>
        public void DeleteOne(T one, bool autoupdate = true)
        {
            var oneItem = new Item() { Value = one };

            var item = DbSet.FirstOrDefault(x => x.State.IsNotIn(ItemState.Removed) && x.Equals(oneItem));
            if (item != null)
            {
                if (!(item.Value.Equals(one) || AttachedList.Contains(one)))
                    throw new InvalidOperationException("item doesn't attached to context");

                item.State = ItemState.Removed;
            }

            if (autoupdate)
                UpdateAll();
        }

        public void Dispose()
        {
            DbSet = null;
            if (AttachedList != null)
            {
                AttachedList.Clear();
                AttachedList = null;
            }
        }
        /// <summary>
        /// Clear all repositary data
        /// </summary>
        public void Clear()
        {
            DbSet.Clear();
        }


        /// <summary>
        /// Get list of entities according to predicate
        /// </summary>
        /// <param name="predicate">filtering predicate</param>
        /// <returns>list of entities</returns>
        public IList<T> GetAll(string filterStr)
        {
            var dict = filterStr.SplitToDictionaryExt("and", "=");
            var query = DbSet.Where(entry =>
            {
                var entryProperties = entry.Value.ToPropertyValuesDictionary(filterDefaultValues: false);
                foreach (var filter in dict)
                {
                    if (!entryProperties.Any(prop => prop.Key == filter.Key && prop.Value.ToString() == filter.Value))
                        return false;
                }

                return true;

            }).Select(entry => entry.Value);

            return query.ToList();
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

            AttachedList.Add(one);

            return one;

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
