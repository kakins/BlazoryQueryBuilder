using BlazorQueryBuilder.ExpressionVisitors.Extensions;
using BlazoryQueryBuilder.Shared.Models;
using FluentAssertions;
using System;
using System.Linq.Expressions;
using Xunit;

namespace BlazorQueryBuilder.Tests.ExpressionVisitors
{
    public class ReplaceLambdaBodyTests
    {
        [Fact]
        public void Replaces_lambda_body()
        {
            // Arrange
            Expression<Func<Person, bool>> originalLambda = person => person.PersonId == "1";
            var lambdaBodyBinary = (BinaryExpression)originalLambda.Body;
            var newBody = lambdaBodyBinary.ReplaceType(ExpressionType.NotEqual);

            // Act
            var newLambda = originalLambda.ReplaceBody(newBody);

            // Assert
            newLambda.Body.Should().Be(newBody);
        }
    }
}
