using BlazorQueryBuilder.ExpressionVisitors;
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
            var newBody = lambdaBodyBinary.ReplaceBinaryType(ExpressionType.NotEqual);

            // Act
            var newLambda = originalLambda.ReplaceLambdaBody(newBody);

            // Assert
            newLambda.Body.Should().Be(newBody);
        }
    }
}
