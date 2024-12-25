using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BlazoryQueryBuilder.Shared.Extensions
{
    public static class TypeExtensions
    {
        public static T GetDefaultValue<T>()
        {
            var e = Expression.Lambda<Func<T>>(Expression.Default(typeof(T)));
            return e.Compile()();
        }

        public static object GetDefaultValue(this Type type)
        {
            ArgumentNullException.ThrowIfNull(type);
            var e = Expression.Lambda<Func<object>>(Expression.Convert(Expression.Default(type), typeof(object)));
            return e.Compile()();
        }

        public static bool IsGenericInstance(this Type type, Type genTypeDef, params Type[] args)
        {
            if (type.GetGenericTypeDefinition() != genTypeDef)
                return false;

            Type[] typeArgs = type.GetGenericArguments();

            if (typeArgs.Length != args.Length)
                return false;

            // Go through the arguments passed in, interpret nulls as "any type"
            for (int i = 0; i != args.Length; i++)
            {
                if (args[i] == null)
                    continue;
                if (args[i] != typeArgs[i])
                    return false;
            }

            return true;
        }

        public static MethodInfo GetGenericMethod(
            this Type type,
            string methodName,
            Type genericTypeDefinition,
            params Type[] genericTypeArgs)
        {
            return type
                .GetMethods()
                .Single(method =>
                    method.Name == methodName
                    && method.GetParameters()
                        .Any(p => p.ParameterType.IsGenericInstance(genericTypeDefinition, genericTypeArgs)));
        }
    }
}
