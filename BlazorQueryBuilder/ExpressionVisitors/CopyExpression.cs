using System.Linq.Expressions;

namespace BlazorQueryBuilder.ExpressionVisitors
{
    public class CopyExpression : ExpressionVisitor, IExpressionVisitor<Expression>
    {
        private readonly Expression _expression;

        internal CopyExpression(Expression expression)
        {
            _expression = expression;
        }

        public Expression Execute()
        {
            return Visit(_expression);
        }
    }
}