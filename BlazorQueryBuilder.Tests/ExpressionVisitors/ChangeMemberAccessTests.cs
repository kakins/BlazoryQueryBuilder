using BlazorQueryBuilder.Visitors;
using BlazoryQueryBuilder.Shared.Models;
using FluentAssertions;
using System.Linq.Expressions;
using Xunit;

namespace BlazorQueryBuilder.Tests.ExpressionVisitors
{
    public class ChangeMemberAccessTests
    {
        [Theory]
        [InlineData("FirstName")]
        [InlineData("Addresses")]
        public void Changes_member_access_for_property(string newPropertyName)
        {
            // Arrange
            // {Param_0.PersonId}
            var personId = Expression.MakeMemberAccess(
                Expression.Parameter(typeof(Person)),
                typeof(Person).GetProperty(nameof(Person.PersonId)));

            // Act
            // {Param_0.FirstName}
            var personLastName = new ChangeMemberProperty(typeof(Person),
                    personId,
                    newPropertyName)
                .Change();

            // Assert
            personLastName.Should().BeAssignableTo<MemberExpression>();
            personLastName.Member.Name.Should().Be(newPropertyName);
        }
    }
}
