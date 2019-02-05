using System;
using System.Linq.Expressions;
using System.Reflection;

namespace BlazorQueryBuilder
{
    public class PredicateFactory
    {
        public Expression<Func<T, bool>> CreateRelationalLambda<T>(
            string propertyName, 
            ParameterExpression parameter, 
            string comparisonValue,
            ExpressionType expressionType)
        {
            PropertyInfo property = typeof(T).GetProperty(propertyName);
            MemberExpression memberAccess = Expression.MakeMemberAccess(parameter, property);

            ConstantExpression right = Expression.Constant(comparisonValue);

            BinaryExpression binary = Expression.MakeBinary(expressionType, memberAccess, right);

            Expression<Func<T, bool>> expression = Expression.Lambda(binary, parameter) as Expression<Func<T, bool>>;

            return expression;
        }
    }
}
