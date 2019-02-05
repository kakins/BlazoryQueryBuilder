using System.Linq.Expressions;

namespace BlazorQueryBuilder.Visitors
{
    public class ReplaceBinaryLeft : ExpressionVisitor
    {
        private readonly Expression _expression;
        private readonly Expression _newLeft;

        public ReplaceBinaryLeft(Expression expression, Expression newLeft)
        {
            _expression = expression;
            _newLeft = newLeft;
        }

        public BinaryExpression Replace()
        {
            return (BinaryExpression)Visit(_expression);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (_expression == node)
            {
                return Expression.MakeBinary(node.NodeType, _newLeft, Visit(node.Right));
            }

            return Visit(node);
        }
    }
}