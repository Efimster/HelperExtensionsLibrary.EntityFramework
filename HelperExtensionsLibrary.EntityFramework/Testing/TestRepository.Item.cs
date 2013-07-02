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
    public partial class TestRepository<T> : ITestRepository<T>
           where T : class
    {
        /// <summary>
        /// Represens item of test repository
        /// </summary>
        public partial class Item : IEquatable<Item>
        {
            public T Value { get; set; }
            public ItemState State { get; set; }
            private static IList<Action<T>> MinMaxLengthConstraintActions { get; set; }
            private static IList<DbGeneratedIdentityAction> DbGeneratedActions { get; set; }
            private static IList<string> KeyProperties { get; set; }


            static Item()
            {
                ForeignKeysActions = GetForeignKeyRelations();
                DbGeneratedActions = GetDatabaseGeneratedIdentitys();
                KeyProperties = GetKeyData();
                MinMaxLengthConstraintActions = GetMaxLengthConstraintActions();//.Union(GetMinLengthConstraintActions()).ToList();
            }

            public Item()
            {
                State = ItemState.Unchanged;
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
