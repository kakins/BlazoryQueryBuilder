using System.Linq.Expressions;

namespace BlazorQueryBuilder.Visitors
{
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