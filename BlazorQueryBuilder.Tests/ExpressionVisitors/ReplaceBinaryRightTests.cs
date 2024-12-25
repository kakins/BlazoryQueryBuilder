using BlazorQueryBuilder.ExpressionVisitors;
using FluentAssertions;
using System.Linq.Expressions;
using Xunit;

namespace BlazorQueryBuilder.Tests.ExpressionVisitors
{
    public class ReplaceBinaryRightTests
    {
        [Fact]
        public void Replaces_right_side_of_logical_binary_expression()
        {
            // Arrange
            var left = Expression.Constant(true);
            var right = Expression.Constant(true);
            var originalExpression = Expression.MakeBinary(ExpressionType.Equal, left, right);

            // Act
            var newExpression = originalExpression.ReplaceBinaryRight(Expression.Constant(false)).Execute();

            // Assert
            newExpression.Should().BeAssignableTo<BinaryExpression>();
            newExpression.Right.Should().BeOfType<ConstantExpression>();
            ((ConstantExpression)newExpression.Right).Value.Should().Be(false);
        }
    }
}
