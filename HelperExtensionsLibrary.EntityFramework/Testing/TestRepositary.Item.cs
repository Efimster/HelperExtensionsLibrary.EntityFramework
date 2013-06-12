using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using HelperExtensionsLibrary.Attributes;
using HelperExtensionsLibrary.IEnumerable;
using HelperExtensionsLibrary.Objects;
using HelperExtensionsLibrary.Collections;
using System.Linq;

namespace HelperExtensionsLibrary.EntityFramework.Testing
{
    public partial class TestRepositary<T> : ITestRepositary<T>
           where T : class
    {
        /// <summary>
        /// Represens item of test repositary
        /// </summary>
        public class Item : IEquatable<Item>
        {
            public T Value { get; set; }
            public ItemState State { get; set; }
            private static IList<Action<T>> ForeignKeysActions { get; set; }
            private static IList<DbGeneratedIdentityAction> DbGeneratedActions { get; set; }
            private static IList<string> KeyProperties { get; set; }


            static Item()
            {
                ForeignKeysActions = GetForeignKeyRelations();
                DbGeneratedActions = GetDatabaseGeneratedIdentitys();
                KeyProperties = GetKeyData();
            }

            public Item()
            {
                State = ItemState.Unchanged;
            }
            /// <summary>
            /// Returns list of constructed foreign key generation action
            /// </summary>
            /// <returns></returns>
            internal static IList<Action<T>> GetForeignKeyRelations()
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
            /// <summary>
            /// Returns list of constructed database identity generation action
            /// </summary>
            /// <returns></returns>
            internal static IList<DbGeneratedIdentityAction> GetDatabaseGeneratedIdentitys()
            {
                var type = typeof(T);
                var dbGeneratedActions = new List<DbGeneratedIdentityAction>();

                var properties = type.GetProperties().FilterPropertiesByAttribute<DatabaseGeneratedAttribute, PropertyInfo>(
                    (attr, prop) => attr.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity, 
                    (attr, prop) => prop
                    );
                DatabaseGeneratedAttribute addAttribute = null;
                var propertiesKeys = type.GetProperties().FilterPropertiesByAttribute<KeyAttribute, PropertyInfo>(
                    (attr, prop) => (addAttribute = prop.GetCustomAttribute<DatabaseGeneratedAttribute>()) == null || addAttribute.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity,
                    (attr, prop) => prop
                    ).ToList();

                if (propertiesKeys.Count > 1)
                    propertiesKeys = new List<PropertyInfo>(1);


                properties = properties.Union(propertiesKeys);

                foreach (var prop in properties)
                {
                    var act = new DbGeneratedIdentityAction(TestRepositaryHelper.ConstructDbGeneratedIdentity<T>(prop.Name));
                    dbGeneratedActions.Add(act);
                }

                return dbGeneratedActions;
            }
            /// <summary>
            /// Returns list of key properties
            /// </summary>
            /// <returns></returns>
            public static IList<string> GetKeyData()
            {
                var type = typeof(T);
                return type.GetProperties().FilterPropertiesByAttribute<KeyAttribute>().Select(prop => prop.Name).ToList();
            }

            public override bool Equals(object obj)
            {
                var item = obj as Item;
                return item != null ? this.Equals(item) : false;
            }
            
            public bool Equals(Item other)
            {

                if (!KeyProperties.IsEmpty())
                {
                    var itemData = Value.ToPropertyValuesDictionary(filterDefaultValues: false);
                    var otherItemData = other.Value.ToPropertyValuesDictionary(filterDefaultValues: false);
                    return KeyProperties.All(keyProperty => ((object)itemData[keyProperty]).Equals(((object)otherItemData[keyProperty])));
                }
                else
                {
                    var itemData = Value.ToPropertyValuesDictionary(filter: descriptor => !descriptor.Attributes.OfType<ForeignKeyAttribute>().Any());
                    var otherItemData = other.Value.ToPropertyValuesDictionary(filter: descriptor => !descriptor.Attributes.OfType<ForeignKeyAttribute>().Any());

                    return itemData.All(item => item.Value.Equals(otherItemData[item.Key]));
                }
                
                //return Value.Equals(other.Value);
            }

            public override int GetHashCode()
            {
                return Value.GetHashCode();
            }
            /// <summary>
            /// Update foreign keys values
            /// </summary>
            public void TriggerForeignKeyActons()
            {
                ForeignKeysActions.ForEach(act => act(Value));
            }
            /// <summary>
            /// Update generated identity keys values
            /// </summary>
            public void TriggerDbGeneratedActons()
            {
                DbGeneratedActions.ForEach(act => act.Do(Value, ++act.NextValue));
            }
        }
        /// <summary>
        /// Encapsulates action and identity key value
        /// </summary>
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
