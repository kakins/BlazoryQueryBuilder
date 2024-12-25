using System.Linq.Expressions;

namespace BlazorQueryBuilder.ExpressionVisitors
{
    public interface IExpressionVisitor<T> where T : Expression
    {
        T Execute();
    }

    public class ExpressionVisitorFactory 
    {
        public static IExpressionVisitor<MemberExpression> ChangePropertyAccess(Type type, Expression expression, string propertyName)
        {
            return new ChangePropertyAccess(type, expression, propertyName);
        }

        public static IExpressionVisitor<BinaryExpression> ReplaceBinaryLeft(BinaryExpression expression, Expression newLeft)
        {
            return new ReplaceBinaryLeft(expression, newLeft);
        }

        public static IExpressionVisitor<BinaryExpression> ReplaceBinaryRight(BinaryExpression expression, Expression newRight)
        {
            return new ReplaceBinaryRight(expression, newRight);
        }

        public static IExpressionVisitor<BinaryExpression> ReplaceBinaryType(BinaryExpression expression, ExpressionType newExpressionType)
        {
            return new ReplaceBinaryType(expression, newExpressionType);
        }

        public static IExpressionVisitor<BinaryExpression> ReplaceBinary(BinaryExpression expression, BinaryExpression newBinary)
        {
            return new ReplaceBinary(expression, newBinary);
        }

        public static IExpressionVisitor<LambdaExpression> ReplaceLambdaBody(LambdaExpression expression, Expression newBody)
        {
            return new ReplaceLambdaBody(expression, newBody);
        }
    }
}
