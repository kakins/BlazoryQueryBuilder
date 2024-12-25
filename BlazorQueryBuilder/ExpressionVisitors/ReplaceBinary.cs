using System.Linq.Expressions;

namespace BlazorQueryBuilder.ExpressionVisitors
{
    public class ReplaceBinary : ExpressionVisitor
    {
        private readonly BinaryExpression _original;
        private readonly BinaryExpression _replacement;

        public ReplaceBinary(BinaryExpression original, BinaryExpression replacement)
        {
            _original = original;
            _replacement = replacement;
        }

        public BinaryExpression Replace()
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