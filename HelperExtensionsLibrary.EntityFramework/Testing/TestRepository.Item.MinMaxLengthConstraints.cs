using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HelperExtensionsLibrary.Attributes;
using HelperExtensionsLibrary.IEnumerable;
using HelperExtensionsLibrary.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace HelperExtensionsLibrary.EntityFramework.Testing
{
    public partial class TestRepository<T> : ITestRepository<T>
          where T : class
    {
        public partial class Item
        {
            /// <summary>
            /// Returns list of max length constraint actions
            /// </summary>
            /// <returns>list of actions</returns>
            internal static IList<Action<T>> GetMaxLengthConstraintActions()
            {
                var type = typeof(T);

                var maxLengthConstraintActions = new List<Action<T>>();

                foreach (var prop in type.GetProperties().FilterPropertiesByAttribute<MaxLengthAttribute>())
                {
                    var attr = prop.GetCustomAttribute<MaxLengthAttribute>();

                    var act = TestRepositoryHelper.ConstructMaxLengthConstraint2<T>(prop.Name, attr.Length, attr.ErrorMessage);
                    maxLengthConstraintActions.Add(act);
                }

                return maxLengthConstraintActions;
            }

            /// <summary>
            /// Returns list of max length constraint actions
            /// </summary>
            /// <returns>list of actions</returns>
            internal static IList<Action<T>> GetMinLengthConstraintActions()
            {
                var type = typeof(T);

                var minLengthConstraintActions = new List<Action<T>>();

                foreach (var prop in type.GetProperties().FilterPropertiesByAttribute<MinLengthAttribute>())
                {
                    var attr = prop.GetCustomAttribute<MinLengthAttribute>();

                    var act = TestRepositoryHelper.ConstructMinLengthConstraint<T>(prop.Name, attr.Length, attr.ErrorMessage);
                    minLengthConstraintActions.Add(act);
                }

                return minLengthConstraintActions;
            }

            /// <summary>
            /// Checks min and max constraints on entity
            /// </summary>
            public void TriggerMinMaxLengthConstraintActons()
            {
                MinMaxLengthConstraintActions.ForEach(act => act(Value));
            }
        }
    }
}
