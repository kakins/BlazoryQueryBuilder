using BlazoryQueryBuilder.Shared.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace BlazoryQueryBuilder.Shared.Util
{
    public class EnumerableMethodInfo
    {
        public static MethodInfo Select<TSource, TResult>()
        {
            return typeof(Enumerable)
                .GetGenericMethod(
                    nameof(Enumerable.Select), typeof(Func<,>),
                    null,
                    null)
                .MakeGenericMethod(typeof(TSource), typeof(TResult));
        }

        public static MethodInfo Any<TSource>()
        {
            return typeof(Enumerable)
                .GetGenericMethod(
                    nameof(Enumerable.Any),
                    typeof(Func<,>),
                    null,
                    typeof(bool))
                .MakeGenericMethod(typeof(TSource));
        }

        public static MethodInfo Contains<TSource>()
        {
            var method = typeof(Enumerable)
                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                .Where(m => m.Name == "Contains" && m.GetParameters().Length == 2)
                .FirstOrDefault();

            var methodGeneric = method.MakeGenericMethod(typeof(TSource));

            return methodGeneric;
        }
    }
}
