using System.Linq.Expressions;

namespace BlazorQueryBuilder.ExpressionVisitors.Extensions
{
    public static class MemberExpressionVisitorExtensions
    {
        public static MemberExpression ChangePropertyAccess(this MemberExpression expression, Type propertyType, string propertyName)
        {
            return new ChangePropertyAccess(expression, propertyType, propertyName).Execute();
        }
    }
}
