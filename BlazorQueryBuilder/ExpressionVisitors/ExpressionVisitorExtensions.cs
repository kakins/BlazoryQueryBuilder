using System.Linq.Expressions;

namespace BlazorQueryBuilder.ExpressionVisitors
{
    public static class ExpressionVisitorExtensions
    {
        public static MemberExpression ChangePropertyAccess(this MemberExpression expression, Type propertyType, string propertyName)
        {
            return new ChangePropertyAccess(expression, propertyType, propertyName).Execute();
        }

        public static BinaryExpression ReplaceBinaryLeft(this BinaryExpression expression, Expression newLeft)
        {
            return new ReplaceBinaryLeft(expression, newLeft).Execute();
        }

        public static BinaryExpression ReplaceBinaryRight(this BinaryExpression expression, Expression newRight)
        {
            return new ReplaceBinaryRight(expression, newRight).Execute();
        }

        public static BinaryExpression ReplaceBinaryType(this BinaryExpression expression, ExpressionType newExpressionType)
        {
            return new ReplaceBinaryType(expression, newExpressionType).Execute();
        }

        public static BinaryExpression ReplaceBinary(this BinaryExpression expression, BinaryExpression newBinary)
        {
            return new ReplaceBinary(expression, newBinary).Execute();
        }

        public static LambdaExpression ReplaceLambdaBody(this LambdaExpression expression, Expression newBody)
        {
            return new ReplaceLambdaBody(expression, newBody).Execute();
        }

        public static Expression CopyExpression(this Expression expression)
        {
            return new CopyExpression(expression).Execute();
        }
    }
}
