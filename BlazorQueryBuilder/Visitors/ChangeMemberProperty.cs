using System;
using System.Linq.Expressions;
using System.Reflection;

namespace BlazorQueryBuilder.Visitors
{
    public class ChangeMemberProperty : ExpressionVisitor
    {
        private readonly Expression _expression;
        private readonly Type _propertyType;
        private readonly string _propertyName;

        public ChangeMemberProperty(Type type, Expression expression, string propertyName)
        {
            _expression = expression;
            _propertyName = propertyName;
            _propertyType = type;
        }

        public MemberExpression Change()
        {
            return (MemberExpression)Visit(_expression);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node == _expression)
            {
                Expression expression = Visit(node.Expression);
                PropertyInfo prop = _propertyType.GetProperty(_propertyName);
                return Expression.MakeMemberAccess(expression, prop);
            }
            return base.VisitMember(node);
        }
    }

    public class ChangeBinaryToMethodCall : ExpressionVisitor
    {
        private readonly Expression _expression;

        public ChangeBinaryToMethodCall(Expression expression)
        {
            _expression = expression;
        }

        public MethodCallExpression Change()
        {
            return (MethodCallExpression) Visit(_expression);
        }

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node == _expression)
            {
                var left = node.Left;
            }

            return base.VisitBinary(node);
        }
    }
}