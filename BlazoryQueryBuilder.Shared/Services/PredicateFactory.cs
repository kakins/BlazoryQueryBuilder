using System;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace BlazoryQueryBuilder.Shared.Services
{
    public class PredicateFactory
    {
        public Expression<Func<T, bool>> CreateRelationalLambda<T>(
            string propertyName, 
            ParameterExpression parameter, 
            object comparisonValue,
            ExpressionType expressionType)
        {
            PropertyInfo property = typeof(T).GetProperty(propertyName);
            MemberExpression memberAccess = Expression.MakeMemberAccess(parameter, property);

            ConstantExpression right = Expression.Constant(comparisonValue);

            BinaryExpression binary = Expression.MakeBinary(expressionType, memberAccess, right);

            Expression<Func<T, bool>> expression = Expression.Lambda(binary, parameter) as Expression<Func<T, bool>>;

            return expression;
        }

        public static Expression<Func<T, bool>> GetLambda<T>(string expression, Type type)
        {
            var config = new ParsingConfig { RenameParameterExpression = true };

            var lambda = DynamicExpressionParser.ParseLambda(
                config,
                type,
                typeof(bool),
                expression) as Expression<Func<T, bool>>;

            return lambda;
        }


    }


}
