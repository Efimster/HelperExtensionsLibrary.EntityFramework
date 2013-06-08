using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using HelperExtensionsLibrary.Attributes;
using HelperExtensionsLibrary.IEnumerable;

namespace HelperExtensionsLibrary.EntityFramework.Testing
{
    public partial class TestRepositary<T> : ITestRepositary<T>
           where T : class
    {
        public class Item : IEquatable<Item>
        {
            public T Value { get; set; }
            public ItemState State { get; set; }
            private static IList<Action<T>> ForeignKeysActions { get; set; }
            private static IList<DbGeneratedIdentityAction> DbGeneratedActions { get; set; }


            static Item()
            {
                ForeignKeysActions = GetForeignKeyRelations();
                DbGeneratedActions = GetDatabaseGeneratedIdentitys();
            }

            public Item()
            {
                State = ItemState.Unchanged;
            }

            private static IList<Action<T>> GetForeignKeyRelations()
            {
                var type = typeof(T);

                var foreignKeyActions = new List<Action<T>>();

                foreach (var prop in type.GetProperties().FilterPropertiesByAttribute<ForeignKeyAttribute>())
                {
                    var attr = prop.GetCustomAttribute<ForeignKeyAttribute>();

                    var act = TestRepositaryHelper.ConstructForeignKeySetter<T>(prop.Name, attr.Name);
                    foreignKeyActions.Add(act);
                }

                return foreignKeyActions;
            }

            private static List<DbGeneratedIdentityAction> GetDatabaseGeneratedIdentitys()
            {
                var type = typeof(T);
                var dbGeneratedActions = new List<DbGeneratedIdentityAction>();

                foreach (var prop in type.GetProperties().FilterPropertiesByAttribute<DatabaseGeneratedAttribute>())
                {
                    var attr = prop.GetCustomAttribute<DatabaseGeneratedAttribute>();
                    if (attr.DatabaseGeneratedOption != DatabaseGeneratedOption.Identity)
                        continue;

                    var act = new DbGeneratedIdentityAction(TestRepositaryHelper.ConstructDbGeneratedIdentity<T>(prop.Name));
                    dbGeneratedActions.Add(act);
                }

                return dbGeneratedActions;
            }

            public override bool Equals(object obj)
            {
                var item = obj as Item;
                return item != null ? this.Equals(item) : false;
            }

            public bool Equals(Item other)
            {
                return Value.Equals(other.Value);
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }

            public void TriggerForeignKeyActons()
            {
                ForeignKeysActions.ForEach(act => act(Value));
            }

            public void TriggerDbGeneratedActons()
            {
                DbGeneratedActions.ForEach(act => act.Do(Value, ++act.NextValue));
            }
        }

        public class DbGeneratedIdentityAction
        {
            public Action<T, int> Do { get; private set; }
            public int NextValue { get; set; }

            public DbGeneratedIdentityAction(Action<T, int> act)
            {
                Do = act;
                NextValue = 0;
            }
        }
    }
}
