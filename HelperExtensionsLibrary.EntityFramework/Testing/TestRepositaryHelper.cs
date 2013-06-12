using System;
using System.Linq.Expressions;
using HelperExtensionsLibrary.Reflection;

namespace HelperExtensionsLibrary.EntityFramework.Testing
{
    public static class TestRepositaryHelper
    {
        /// <summary>
        /// Determines whether Item has given state
        /// </summary>
        /// <param name="state">item state</param>
        /// <param name="instate">checked state</param>
        /// <returns></returns>
        public static bool IsIn(this ItemState state, ItemState instate)
        {
            return (state & instate) != ItemState.None;
        }
        /// <summary>
        /// Determines whether Item hasn't given state
        /// </summary>
        /// <param name="state">item state</param>
        /// <param name="instate">checked state</param>
        /// <returns></returns>
        public static bool IsNotIn(this ItemState state, ItemState instate)
        {
            return (state & instate) == ItemState.None;
        }

        /// <summary>
        /// Construct forreign key setter action
        /// </summary>
        /// <typeparam name="TObj">Type of entity</typeparam>
        /// <param name="complexProperty">Navigate object property</param>
        /// <param name="simpleProperty">Foreign key property</param>
        /// <returns>setter</returns>
        public static Action<TObj> ConstructForeignKeySetter<TObj>(string complexProperty, string simpleProperty) where TObj : class
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
        /// <summary>
        /// Construct DB generated identity property setter
        /// </summary>
        /// <typeparam name="TObj">Entity type</typeparam>
        /// <param name="dbGenratedPropertyName">Database generated property name</param>
        /// <returns>property setter</returns>
        public static Action<TObj, int> ConstructDbGeneratedIdentity<TObj>(string dbGenratedPropertyName) where TObj : class
        {
            ParameterExpression objParam = Expression.Parameter(typeof(TObj), "TObj");
            MemberExpression property = Expression.PropertyOrField(objParam, dbGenratedPropertyName);
            ParameterExpression nextValueParam = Expression.Parameter(typeof(int), "NextValue");

            BinaryExpression ass = Expression.Assign(property, nextValueParam);
            var setter = Expression.Lambda<Action<TObj, int>>(ass, objParam, nextValueParam);
            return setter.Compile();
        }
    }
}
