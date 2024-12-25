using BlazorQueryBuilder.ExpressionVisitors;
using FluentAssertions;
using System.Linq.Expressions;
using Xunit;

namespace BlazorQueryBuilder.Tests.ExpressionVisitors
{
    public class ReplaceBinaryLeftTests
    {
        [Fact]
        public void Replaces_left_side_of_logical_binary_expression()
        {
            // Arrange
            var left = Expression.Constant(true);
            var right = Expression.Constant(true);
            var originalExpression = Expression.MakeBinary(ExpressionType.Equal, left, right);

            // Act
            var newExpression = originalExpression.ReplaceLeft(Expression.Constant(false));

            // Assert
            newExpression.Should().BeAssignableTo<BinaryExpression>();
            newExpression.Left.Should().BeOfType<ConstantExpression>();
            ((ConstantExpression)newExpression.Left).Value.Should().Be(false);
        }
    }
}
