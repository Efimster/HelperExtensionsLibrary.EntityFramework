using System;
using System.Linq.Expressions;
using HelperExtensionsLibrary.Reflection;

namespace HelperExtensionsLibrary.EntityFramework.Testing
{
    public static class TestRepositaryHelper
    {
        public static bool IsIn(this ItemState state, ItemState instate)
        {
            return state == instate;
        }

        public static bool IsNotIn(this ItemState state, ItemState instate)
        {
            return state != instate;
        }


        public static Action<TObj> ConstructForeignKeySetter<TObj>(string complexProperty, string simpleProperty) where TObj : class
        {
            ParameterExpression obj = Expression.Parameter(typeof(TObj), "Tobj");
            var complexMember = ReflectionExtenbtions.ConstructFieldOrPropertyGetter(obj, string.Concat(complexProperty, ".", simpleProperty));

            var complexMemberParent = ReflectionExtenbtions.ConstructFieldOrPropertyGetter(obj, complexProperty);
            var simpleMember = ReflectionExtenbtions.ConstructFieldOrPropertyGetter(obj, simpleProperty);


            var res = Expression.IfThenElse(Expression.NotEqual(complexMemberParent, Expression.Constant(null)), //if 
                            Expression.Assign(simpleMember, complexMember),//then
                        Expression.Block(   //else
                             Expression.Assign(complexMemberParent, Expression.New(complexMemberParent.Type)),
                             Expression.Assign(complexMember, simpleMember)
                ));

            var setter = Expression.Lambda<Action<TObj>>(res, obj);

            return setter.Compile();
        }

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
