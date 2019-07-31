using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BlazoryQueryBuilder.Shared.Extensions;
using BlazoryQueryBuilder.Shared.Models;

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
    }
}
