using System.Linq.Expressions;

namespace BlazorQueryBuilder.ExpressionVisitors
{
    public class ReplaceBinaryRight : ExpressionVisitor, IExpressionVisitor<BinaryExpression>
    {
        private readonly BinaryExpression _expression;
        private readonly Expression _newRight;

        internal ReplaceBinaryRight(BinaryExpression expression, Expression newRight)
        {
            _expression = expression;
            _newRight = newRight;
        }

        public BinaryExpression Execute()
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
}