using BlazoryQueryBuilder.Shared.Models;
using System.Linq.Expressions;
using System.Reflection;

namespace BlazorQueryBuilder.ExpressionVisitors
{
    // This class is not really used yet and not fully implemented
    // The idea is to append a logical binary expression to an existing lambda expression
    // Unsure if this expression should be constrainted to a predicate or more open ended
    public class AddLogicalBinaryLambda : ExpressionVisitor, IExpressionVisitor<Expression>
    {
        private readonly Expression _originalExpression;
        private readonly ExpressionType _binaryExpressionType;

        public AddLogicalBinaryLambda(Expression originalExpression, ExpressionType binaryExpressionType = ExpressionType.AndAlso)
        {
            _originalExpression = originalExpression;
            _binaryExpressionType = binaryExpressionType;
        }

        public Expression Execute()
        {
            return Visit(_originalExpression);
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
