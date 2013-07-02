using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using HelperExtensionsLibrary.Attributes;
using HelperExtensionsLibrary.IEnumerable;
using System.Linq;

namespace HelperExtensionsLibrary.EntityFramework.Testing
{
   public partial class TestRepository<T> : ITestRepository<T>
           where T : class
    {
       public partial class Item
       {
           /// <summary>
           /// Returns list of constructed database identity generation action
           /// </summary>
           /// <returns>list of actions</returns>
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
                   var act = new DbGeneratedIdentityAction(TestRepositoryHelper.ConstructDbGeneratedIdentity<T>(prop.Name));
                   dbGeneratedActions.Add(act);
               }

               return dbGeneratedActions;
           }

           /// <summary>
           /// Update generated identity keys values
           /// </summary>
           public void TriggerDbGeneratedActons()
           {
               DbGeneratedActions.ForEach(act => act.Do(Value, ++act.NextValue));
           }
       }


    }
}
