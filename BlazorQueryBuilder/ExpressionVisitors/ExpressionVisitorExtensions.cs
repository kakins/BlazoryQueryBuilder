using System.Linq.Expressions;

namespace BlazorQueryBuilder.ExpressionVisitors
{
    public static class MemberExpressionVisitorExtensions
    {
        public static MemberExpression ChangePropertyAccess(this MemberExpression expression, Type propertyType, string propertyName)
        {
            return new ChangePropertyAccess(expression, propertyType, propertyName).Execute();
        }
    }

    public static class BinaryExpressionVisitorExtensions
    {
        public static BinaryExpression ReplaceLeft(this BinaryExpression expression, Expression newLeft)
        {
            return new ReplaceBinaryLeft(expression, newLeft).Execute();
        }

        public static BinaryExpression ReplaceRight(this BinaryExpression expression, Expression newRight)
        {
            return new ReplaceBinaryRight(expression, newRight).Execute();
        }

        public static BinaryExpression ReplaceType(this BinaryExpression expression, ExpressionType newBinaryType)
        {
            return new ReplaceBinaryType(expression, newBinaryType).Execute();
        }

        public static BinaryExpression Replace(this BinaryExpression expression, BinaryExpression newExpression)
        {
            return new ReplaceBinary(expression, newExpression).Execute();
        }
    }

    public static class LambdaExpressionVisitorExtensions
    {
        public static LambdaExpression ReplaceBody(this LambdaExpression expression, Expression newBody)
        {
            return new ReplaceLambdaBody(expression, newBody).Execute();
        }
    }

    public static class ExpressionVisitorExtensions
    {
        public static Expression Copy(this Expression expression)
        {
            return new CopyExpression(expression).Execute();
        }
    }
}
