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
            // We want an Func<T> which returns the default.
            // Create that expression here.
            Expression<Func<T>> e = Expression.Lambda<Func<T>>(
                // The default value, always get what the *code* tells us.
                Expression.Default(typeof(T))
            );

            // Compile and return the value.
            return e.Compile()();
        }

        public static object GetDefaultValue(this Type type)
        {
            // Validate parameters.
            if (type == null) throw new ArgumentNullException("type");

            // We want an Func<object> which returns the default.
            // Create that expression here.
            Expression<Func<object>> e = Expression.Lambda<Func<object>>(
                // Have to convert to object.
                Expression.Convert(
                    // The default value, always get what the *code* tells us.
                    Expression.Default(type), typeof(object)
                )
            );

            // Compile and return the value.
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
