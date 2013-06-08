using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelperExtensionsLibrary.Attributes;

namespace HelperExtensionsLibrary.EntityFramework.Testing
{
    public partial class TestRepositary<T> : ITestRepositary<T>
           where T : class
    {

        public IList<Item> DbSet { get; private set; }
        private int CurrId { get; set; }

        public TestRepositary()
        {
            DbSet = new List<Item>();
            CurrId = 0;
        }

        private void Init()
        {
            var properties = typeof(T).GetProperties();
            var foreignKeyProperties = properties.FilterPropertiesByAttribute<ForeignKeyAttribute>();
        }


        public IList<T> GetAll(System.Linq.Expressions.Expression<Func<T, bool>> predicate)
        {
            var func = predicate.Compile();
            return DbSet.Where(item => item.State.IsNotIn(ItemState.Removed))
                .Select(item => item.Value)
                .Where(func).ToList();
        }

        public T GetOne(System.Linq.Expressions.Expression<Func<T, bool>> predicate, bool single = true)
        {
            return single ?
                    DbSet.Where(item => item.State.IsNotIn(ItemState.Removed))
                        .Select(item => item.Value).SingleOrDefault(predicate.Compile())
                    : DbSet.Where(item => item.State.IsNotIn(ItemState.Removed))
                        .Select(item => item.Value).FirstOrDefault(predicate.Compile());

        }

        public void AddOne(T one, bool autoupdate = true)
        {
            var item = new Item { Value = one, State = ItemState.Added };
            item.TriggerForeignKeyActons();
            DbSet.Add(item);

            if (autoupdate)
                UpdateAll();
        }

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

                item.TriggerDbGeneratedActons();
                item.State = ItemState.Unchanged;
            }
        }

        public void DeleteOne(T one, bool autoupdate = true)
        {
            var item = DbSet.FirstOrDefault(x => x.State.IsNotIn(ItemState.Removed) && x.Value == one);
            if (item != null)
                item.State = ItemState.Removed;
        }

        public void Dispose()
        {

        }

        public void Clear()
        {
            DbSet.Clear();
        }


        public int NextId()
        {
            return ++CurrId;
        }
    }

    public enum ItemState
    {
        Unchanged,
        Added,
        Removed,
        Changed

    }
}
