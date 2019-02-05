using System.Linq.Expressions;

namespace BlazorQueryBuilder.Visitors
{
    public class ReplaceBinaryType : ExpressionVisitor
    {
        private readonly BinaryExpression _expression;
        private readonly ExpressionType _newExpressionType;

        public ReplaceBinaryType(BinaryExpression expression, ExpressionType newExpressionType)
        {
            _expression = expression;
            _newExpressionType = newExpressionType;
        }

        public BinaryExpression Replace()
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