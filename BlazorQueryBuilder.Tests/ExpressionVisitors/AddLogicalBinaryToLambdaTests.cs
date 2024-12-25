using BlazorQueryBuilder.ExpressionVisitors;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using FluentAssertions;
using System.Linq.Expressions;
using Xunit;

namespace BlazorQueryBuilder.Tests.ExpressionVisitors
{
    public class AddLogicalBinaryToLambdaTests
    {
        [Theory]
        [InlineData("LastName", "Jones", ExpressionType.Equal)]
        [InlineData("LastName", "Jones", ExpressionType.NotEqual)]
        public void Adds_logical_binary_expression_to_lambda(string propertyName, string propertyValue, ExpressionType expressionType)
        {
            // Arrange
            // p => p.LastName == "Jones";
            var parameter = Expression.Parameter(typeof(Person));

            var originalLambda = new PredicateFactory().CreateRelationalPredicate<Person>(
                propertyName,
                parameter,
                propertyValue,
                expressionType);

            // Act
            // p => p.PersonId == "" && p.LastName == "Jones";
            var newLambda = (LambdaExpression)new AddLogicalBinaryLambda(originalLambda).Execute();

            // Assert
            newLambda.Should().NotBeNull();
            newLambda.NodeType.Should().Be(ExpressionType.Lambda);
            newLambda.Body.NodeType.Should().Be(ExpressionType.AndAlso);
            newLambda.Body.Should().BeAssignableTo<BinaryExpression>();
            ((BinaryExpression)newLambda.Body).Left.NodeType.Should().Be(ExpressionType.Equal);
            ((BinaryExpression)newLambda.Body).Right.NodeType.Should().Be(expressionType);
        }
    }
}
