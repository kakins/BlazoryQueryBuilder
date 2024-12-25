using System.Linq.Expressions;

namespace BlazorQueryBuilder.ExpressionVisitors
{
    public interface IExpressionVisitor<T> where T : Expression
    {
        T Execute();
    }

    public static class ExpressionVisitorExtensions
    {
        public static IExpressionVisitor<MemberExpression> ChangePropertyAccess(this MemberExpression expression, Type propertyType, string propertyName)
        {
            return new ChangePropertyAccess(expression, propertyType, propertyName);
        }

        public static IExpressionVisitor<BinaryExpression> ReplaceBinaryLeft(this BinaryExpression expression, Expression newLeft)
        {
            return new ReplaceBinaryLeft(expression, newLeft);
        }

        public static IExpressionVisitor<BinaryExpression> ReplaceBinaryRight(this BinaryExpression expression, Expression newRight)
        {
            return new ReplaceBinaryRight(expression, newRight);
        }

        public static IExpressionVisitor<BinaryExpression> ReplaceBinaryType(this BinaryExpression expression, ExpressionType newExpressionType)
        {
            return new ReplaceBinaryType(expression, newExpressionType);
        }

        public static IExpressionVisitor<BinaryExpression> ReplaceBinary(this BinaryExpression expression, BinaryExpression newBinary)
        {
            return new ReplaceBinary(expression, newBinary);
        }

        public static IExpressionVisitor<LambdaExpression> ReplaceLambdaBody(this LambdaExpression expression, Expression newBody)
        {
            return new ReplaceLambdaBody(expression, newBody);
        }
    }
}
