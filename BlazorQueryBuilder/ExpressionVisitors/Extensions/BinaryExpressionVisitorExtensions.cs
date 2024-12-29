using System.Linq.Expressions;

namespace BlazorQueryBuilder.ExpressionVisitors.Extensions
{
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
}
