using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq.Expressions;
using System.Reflection;
using HelperExtensionsLibrary.Attributes;
using HelperExtensionsLibrary.IEnumerable;
using HelperExtensionsLibrary.Reflection;
using System.Linq;

namespace HelperExtensionsLibrary.EntityFramework.Testing
{
    public partial class TestRepository<T> : ITestRepository<T>
           where T : class
    {
        public partial class Item
        {
            private static IList<Action<T>> ForeignKeysActions { get; set; }

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

                    var act = TestRepositoryHelper.ConstructForeignKeyTrigger<T>(prop.Name, attr.Name);
                    foreignKeyActions.Add(act);
                }

                return foreignKeyActions;
            }

            /// <summary>
            /// Update foreign keys values
            /// </summary>
            public void TriggerForeignKeyActons()
            {
                ForeignKeysActions.ForEach(act => act(Value));
            }
        }
    }

    public static partial class TestRepositoryHelper
    {
        /// <summary>
        /// Construct forreign key setter action
        /// </summary>
        /// <typeparam name="TObj">Type of entity</typeparam>
        /// <param name="complexProperty">Navigate object property</param>
        /// <param name="simpleProperty">Foreign key property</param>
        /// <returns>setter</returns>
        public static Action<TObj> ConstructForeignKeyTrigger<TObj>(string complexProperty, string simpleProperty) where TObj : class
        {
            ParameterExpression obj = Expression.Parameter(typeof(TObj), "Tobj");
            var complexMember = ReflectionExtensions.ConstructFieldOrPropertyGetter(obj, string.Concat(complexProperty, ".", simpleProperty));

            var complexMemberParent = ReflectionExtensions.ConstructFieldOrPropertyGetter(obj, complexProperty);
            var simpleMember = ReflectionExtensions.ConstructFieldOrPropertyGetter(obj, simpleProperty);


            var res = Expression.IfThenElse(Expression.NotEqual(complexMemberParent, Expression.Constant(null)), //if 
                            Expression.Assign(simpleMember, complexMember),//then
                        Expression.Block(   //else
                             Expression.Assign(complexMemberParent, Expression.New(complexMemberParent.Type)),
                             Expression.Assign(complexMember, simpleMember)
                ));

            var setter = Expression.Lambda<Action<TObj>>(res, obj);

            return setter.Compile();
        }

        //public static Expression GetForeinKeyValue<T, TProperty>(Expression<Func<T, TProperty>> property)
        //{
        //    var type = typeof(T);
        //    var keyName = type.GetRuntimeProperty(typeof(TProperty).Name).GetCustomAttribute<ForeignKeyAttribute>().Name;
            
        //    MemberExpression getter = ReflectionExtensions.ConstructFieldOrPropertyGetter(Expression.Parameter(type), keyName);
            
        //    return null;
        //}

        public static Expression<Func<T, TProperty, bool>> GetForeinKeyPredicate<T, TProperty>(Expression<Func<T, TProperty>> path, out Action<T,TProperty> setter)
        {
            var type = typeof(T);
            var typeProperty = typeof(TProperty);
            var foreignKeyEntityMember = ((MemberExpression)path.Body).Member;
            var keyName = foreignKeyEntityMember.CustomAttributes.First(attrData => attrData.AttributeType == typeof(ForeignKeyAttribute)).ConstructorArguments[0].Value.ToString();
            var baseEntityParameter = Expression.Parameter(type, "TBaseEntity");
            var foreignEntityParameter = Expression.Parameter(typeProperty, "TForeignEntity");
            
            MemberExpression foreignKeyGetter = ReflectionExtensions.ConstructFieldOrPropertyGetter(baseEntityParameter, keyName);
            MemberExpression primaryKeyGetter = ReflectionExtensions.ConstructFieldOrPropertyGetter(foreignEntityParameter, keyName);

            var binary = Expression.Equal(foreignKeyGetter, primaryKeyGetter);

            setter = ReflectionExtensions.ConstructFieldOrPropertySetter<T, TProperty>(foreignKeyEntityMember.Name);

            return Expression.Lambda<Func<T, TProperty, bool>>(binary, baseEntityParameter, foreignEntityParameter);

        }


    }
}
