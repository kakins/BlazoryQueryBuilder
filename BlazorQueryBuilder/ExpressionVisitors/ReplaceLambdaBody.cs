using System.Linq;
using System.Linq.Expressions;

namespace BlazorQueryBuilder.ExpressionVisitors
{
    public class ReplaceLambdaBody : ExpressionVisitor
    {
        private readonly LambdaExpression _lambda;
        private readonly Expression _newBody;

        public ReplaceLambdaBody(LambdaExpression lambda, Expression newBody)
        {
            _lambda = lambda;
            _newBody = newBody;
        }

        public LambdaExpression Replace()
        {
            return (LambdaExpression)Visit(_lambda);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {

            if (node == _lambda)
            {
                var parameters = node.Parameters.Select(p => (ParameterExpression)Visit(p)).ToArray();
                return Expression.Lambda(_newBody, parameters);
            }

            return base.VisitLambda(node);
        }
    }
}