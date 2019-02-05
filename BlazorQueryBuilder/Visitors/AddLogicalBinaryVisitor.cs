using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BlazorQueryBuilder.Models;

namespace BlazorQueryBuilder.Visitors
{
    public class AddLogicalBinaryVisitor : ExpressionVisitor
    {
        public Expression Add(Expression expression)
        {
            return Visit(expression);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            PropertyInfo property = typeof(Worker).GetProperty(typeof(Worker).GetProperties().First().Name);
            Console.WriteLine("new constant prop: " + property.Name);
            Expression parameter = Visit(node.Parameters[0]);
            MemberExpression memberAccess = Expression.MakeMemberAccess(parameter, property);

            var newLeft = Expression.MakeBinary(ExpressionType.Equal, memberAccess, Expression.Constant(string.Empty));

            Expression newRight = Visit(node.Body);

            var newBody = Expression.MakeBinary(ExpressionType.AndAlso, newLeft, newRight);

            return Expression.Lambda<T>(newBody, (ParameterExpression)parameter);
        }
    }
}
