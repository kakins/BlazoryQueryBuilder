﻿using System.Linq.Expressions;

namespace BlazorQueryBuilder.ExpressionVisitors
{
    public class ReplaceBinaryLeft : ExpressionVisitor
    {
        private readonly BinaryExpression _expression;
        private readonly Expression _newLeft;

        public ReplaceBinaryLeft(BinaryExpression expression, Expression newLeft)
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

            // TODO: Consider using base.VisitBinary(node) instead of Visit(node)
            return Visit(node);
        }
    }
}