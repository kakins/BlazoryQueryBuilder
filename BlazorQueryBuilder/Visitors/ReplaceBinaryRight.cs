using System.Linq.Expressions;

namespace BlazorQueryBuilder.Visitors
{
    public class ReplaceBinaryRight : ExpressionVisitor
    {
        private readonly BinaryExpression _expression;
        private readonly Expression _newRight;

        public ReplaceBinaryRight(BinaryExpression expression, Expression newRight)
        {
            _expression = expression;
            _newRight = newRight;
        }

        public BinaryExpression Replace()
        {
            return (BinaryExpression)Visit(_expression);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (_expression == node)
            {
                return Expression.MakeBinary(node.NodeType, Visit(node.Left), _newRight);
            }

            return Visit(node);
        }
    }

    public class CopyExpression : ExpressionVisitor
    {
        private readonly Expression _expression;

        public CopyExpression(Expression expression)
        {
            _expression = expression;
        }

        public Expression Copy()
        {
            return Visit(_expression);
        }
    }

}