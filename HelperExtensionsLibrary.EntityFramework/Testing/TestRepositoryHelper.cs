using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Linq.Expressions;
using HelperExtensionsLibrary.Reflection;

namespace HelperExtensionsLibrary.EntityFramework.Testing
{
    public static partial class TestRepositoryHelper
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

        /// <summary>
        /// Builds checking constraint action
        /// </summary>
        /// <typeparam name="TObj">Entity type</typeparam>
        /// <param name="maxLenghtPropertyName">property name with Max Length constraint</param>
        /// <param name="maxLength">max length</param>
        /// <param name="errMessage">error message</param>
        /// <returns>checking constraint action</returns>
        public static Action<TObj> ConstructMaxLengthConstraint<TObj>(string maxLenghtPropertyName, int maxLength, string errMessage) where TObj : class
        {
            var constructor = typeof(UpdateException).GetConstructor(new[]{typeof(string)});

            LabelTarget returnTarget = Expression.Label();
            LabelExpression returnLabel = Expression.Label(returnTarget);
            
            ParameterExpression objParam = Expression.Parameter(typeof(TObj), "TObj");
            MemberExpression property = Expression.PropertyOrField(objParam, maxLenghtPropertyName);
            var checkOnNull = Expression.IfThen(Expression.Equal(property, Expression.Constant(null)), Expression.Return(returnTarget));


            var converted = Expression.Call(Expression.PropertyOrField(objParam, maxLenghtPropertyName), "ToString", null, null);
            property = Expression.PropertyOrField(converted, "Length");

            var checkConstraint = Expression.IfThen(Expression.GreaterThan(property, Expression.Constant(maxLength)), //if 
                            Expression.Throw(Expression.New(constructor, Expression.Constant(errMessage ?? string.Empty)))//then
                            );

            var block = Expression.Block(checkOnNull, checkConstraint, returnLabel);

            var setter = Expression.Lambda<Action<TObj>>(checkConstraint, objParam);
            return setter.Compile();
        }

        public static Action<TObj> ConstructMaxLengthConstraint2<TObj>(string maxLenghtPropertyName, int maxLength, string errMessage) where TObj : class
        {
            var constructor = typeof(UpdateException).GetConstructor(new[] { typeof(string) });


            ParameterExpression objParam = Expression.Parameter(typeof(TObj), "TObj");
            
            var converted = Expression.Call(Expression.PropertyOrField(objParam, maxLenghtPropertyName), "ToString", null, null);
            MemberExpression property = Expression.PropertyOrField(converted, "Length");

            var checkConstraint = Expression.IfThen(Expression.GreaterThan(property, Expression.Constant(maxLength)), //if 
                            Expression.Throw(Expression.New(constructor, Expression.Constant(errMessage ?? string.Empty)))//then
                            );

            property = Expression.PropertyOrField(objParam, maxLenghtPropertyName);
            var checkOnNull = Expression.IfThen(Expression.NotEqual(property, Expression.Constant(null)), checkConstraint);


            var setter = Expression.Lambda<Action<TObj>>(checkOnNull, objParam);
            return setter.Compile();
        }

        /// <summary>
        /// Builds checking constraint action
        /// </summary>
        /// <typeparam name="TObj">Entity type</typeparam>
        /// <param name="minLenghtPropertyName">property name with Max Length constraint</param>
        /// <param name="minLength">min length</param>
        /// <param name="errMessage">error message</param>
        /// <returns>checking constraint action</returns>
        public static Action<TObj> ConstructMinLengthConstraint<TObj>(string minLenghtPropertyName, int minLength, string errMessage) where TObj : class
        {
            var constructor = typeof(UpdateException).GetConstructor(new[] { typeof(string) });

            LabelTarget returnTarget = Expression.Label();
            LabelExpression returnLabel = Expression.Label(returnTarget);
            
            ParameterExpression objParam = Expression.Parameter(typeof(TObj), "TObj");
            MemberExpression property = Expression.PropertyOrField(objParam, minLenghtPropertyName);
            var checkOnNull = Expression.IfThen(Expression.Equal(property, Expression.Constant(null)), Expression.Return(returnTarget));

            var converted = Expression.Call(Expression.PropertyOrField(objParam, minLenghtPropertyName), "ToString", null, null);
            property = Expression.PropertyOrField(converted, "Length");

            var checkConstraint = Expression.IfThen(Expression.LessThan(property, Expression.Constant(minLength)), //if 
                            Expression.Throw(Expression.New(constructor, Expression.Constant(errMessage ?? string.Empty)))//then
                            );
            var block = Expression.Block(checkOnNull, checkConstraint, returnLabel);

            var setter = Expression.Lambda<Action<TObj>>(checkConstraint, objParam);
            return setter.Compile();
        }
        /// <summary>
        /// Specifies the related objects to include in the query results. Differs Entity Framework and Test repositories
        /// </summary>
        /// <typeparam name="T">Entity type</typeparam>
        /// <typeparam name="TProperty">related object type</typeparam>
        /// <param name="source">The source System.Linq.IQueryable<T> on which to call Include.</param>
        /// <param name="includePath">The related object to return in the query results.</param>
        /// <param name="foreignSource">source of foreign type entities</param>
        /// <returns></returns>
        public static IQueryable<T> Include<T, TProperty>(this IQueryable<T> source, Expression<Func<T, TProperty>> includePath, IEnumerable<TProperty> foreignSource) 
            where TProperty : class
            where T : class
        {

            if (source as System.Data.Entity.Infrastructure.DbQuery<T> != null)
                return System.Data.Entity.QueryableExtensions.Include(source, includePath);
            
            Action<T, TProperty> setter;

            var foreignKeyPredicate = TestRepositoryHelper.GetForeinKeyPredicate(includePath, out setter).Compile();

            var query2 = source.AsEnumerable()
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

            return query2.AsQueryable();
        }

    }
}
