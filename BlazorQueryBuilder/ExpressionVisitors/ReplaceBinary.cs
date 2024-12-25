using System.Linq.Expressions;

namespace BlazorQueryBuilder.ExpressionVisitors
{
    public class ReplaceBinary : ExpressionVisitor, IExpressionVisitor<BinaryExpression>
    {
        private readonly BinaryExpression _original;
        private readonly BinaryExpression _replacement;

        internal ReplaceBinary(BinaryExpression original, BinaryExpression replacement)
        {
            _original = original;
            _replacement = replacement;
        }

        public BinaryExpression Execute()
        {
            return (BinaryExpression)Visit(_original);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node == _original)
            {
                return _replacement;
            }

            return base.VisitBinary(node);
        }
    }
}