﻿using BlazorQueryBuilder.ExpressionVisitors;
using FluentAssertions;
using System.Linq.Expressions;
using Xunit;

namespace BlazorQueryBuilder.Tests.ExpressionVisitors
{
    public class ChangeBinaryTests
    {
        [Fact]
        public void Replaces_binary_expression()
        {
            // Arrange
            var originalExpression = Expression.MakeBinary(
                ExpressionType.Equal,
                Expression.Constant(1),
                Expression.Constant(1));

            var newExpression = Expression.MakeBinary(
                ExpressionType.Equal,
                Expression.Constant(2),
                Expression.Constant(2));

            // Act
            var result = originalExpression.ReplaceBinary(newExpression).Execute();

            // Assert
            result.Should().Be(newExpression);
        }
    }
}