using System;
using System.Linq.Expressions;
using System.Reflection;

namespace BlazorQueryBuilder.ExpressionVisitors
{
    public class ChangePropertyAccess : ExpressionVisitor, IExpressionVisitor<MemberExpression>
    {
        private readonly Expression _expression;
        private readonly Type _propertyType;
        private readonly string _propertyName;

        internal ChangePropertyAccess(Type type, Expression expression, string propertyName)
        {
            _expression = expression;
            _propertyName = propertyName;
            _propertyType = type;
        }

        public MemberExpression Execute()
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
}