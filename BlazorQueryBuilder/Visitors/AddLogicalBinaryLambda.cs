using BlazoryQueryBuilder.Shared.Models;
using System.Linq.Expressions;
using System.Reflection;

namespace BlazorQueryBuilder.Visitors
{
    public class AddLogicalBinaryLambda : ExpressionVisitor
    {
        private readonly ExpressionType _binaryExpressionType;

        public AddLogicalBinaryLambda(ExpressionType binaryExpressionType = ExpressionType.AndAlso)
        {
            _binaryExpressionType = binaryExpressionType;
        }

        public Expression Add(Expression expression)
        {
            return Visit(expression);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            PropertyInfo property = typeof(Person).GetProperty(typeof(Person).GetProperties().First().Name);
            Console.WriteLine("new constant prop: " + property.Name);
            Expression parameter = Visit(node.Parameters[0]);
            MemberExpression memberAccess = Expression.MakeMemberAccess(parameter, property);

            var newLeft = Expression.MakeBinary(ExpressionType.Equal, memberAccess, Expression.Constant(string.Empty));

            Expression newRight = Visit(node.Body);

            var newBody = Expression.MakeBinary(_binaryExpressionType, newLeft, newRight);

            return Expression.Lambda<T>(newBody, (ParameterExpression)parameter);
        }
    }
}
