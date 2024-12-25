using System.Linq.Expressions;

namespace BlazorQueryBuilder.ExpressionVisitors
{
    public class ReplaceBinaryType : ExpressionVisitor, IExpressionVisitor<BinaryExpression>
    {
        private readonly BinaryExpression _expression;
        private readonly ExpressionType _newExpressionType;

        internal ReplaceBinaryType(BinaryExpression expression, ExpressionType newExpressionType)
        {
            _expression = expression;
            _newExpressionType = newExpressionType;
        }

        public BinaryExpression Execute()
        {
            return (BinaryExpression)Visit(_expression);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node == _expression)
            {
                return Expression.MakeBinary(_newExpressionType, Visit(node.Left), Visit(node.Right));
            }

            return base.VisitBinary(node);
        }

    }
}