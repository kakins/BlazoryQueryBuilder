using BlazorQueryBuilder.ExpressionVisitors;
using BlazoryQueryBuilder.Shared.Models;
using FluentAssertions;
using System.Linq.Expressions;
using Xunit;

namespace BlazorQueryBuilder.Tests.ExpressionVisitors
{
    public class ChangePropertyAccessTests
    {
        [Theory]
        [InlineData("FirstName")]
        [InlineData("Addresses")]
        public void Changes_property_access(string newPropertyName)
        {
            // Arrange
            // {Param_0.PersonId}
            var personId = Expression.MakeMemberAccess(
                Expression.Parameter(typeof(Person)),
                typeof(Person).GetProperty(nameof(Person.PersonId)));

            // Act
            // {Param_0.FirstName}
            var personLastName = ExpressionVisitorFactory.ChangePropertyAccess(personId,
                    typeof(Person),
                    newPropertyName)
                .Execute();

            // Assert
            personLastName.Should().BeAssignableTo<MemberExpression>();
            personLastName.Member.Name.Should().Be(newPropertyName);
        }
    }
}
