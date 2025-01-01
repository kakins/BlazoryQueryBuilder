using BlazorQueryBuilder.Pages;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace BlazorQueryBuilder.Tests.Pages
{
    public class PredicateTests : TestContext
    {
        public PredicateTests()
        {
            Services.AddSingleton<PredicateFactory>();

            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;
            JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);

            RenderComponent<MudPopoverProvider>();
        }

        [Theory]
        [MemberData(nameof(RelationalPredicateTestData))]
        public async Task Initialiazes_relational_predicate(Expression<Func<Address, bool>> lambdaExpression)
        {
            // Arrange
            // Act
            var component = RenderComponent<BlazorQueryBuilder.Pages.Predicate>(parameters =>
            {
                parameters
                    .Add(p => p.Expression, lambdaExpression.Body)
                    .Add(p => p.ParameterExpression, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Assert
            var logicalPredicate = component.FindComponent<RelationalPredicate>();
            logicalPredicate.Instance.PredicateExpression.Should().Be(lambdaExpression.Body);
            logicalPredicate.Instance.ParameterExpression.Should().Be(lambdaExpression.Parameters[0]);
        }

        [Theory]
        [MemberData(nameof(LogicalPredicateTestData))]
        public async Task Initializes_logical_predicate(Expression<Func<Person, bool>> lambdaExpression)
        {
            // Arrange
            // Act
            var component = RenderComponent<BlazorQueryBuilder.Pages.Predicate>(parameters =>
            {
                parameters
                    .Add(p => p.Expression, lambdaExpression.Body as BinaryExpression)
                    .Add(p => p.ParameterExpression, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Assert
            var logicalPredicate = component.FindComponent<LogicalPredicate>();
            logicalPredicate.Instance.PredicateExpression.Should().Be(lambdaExpression.Body as BinaryExpression);
            logicalPredicate.Instance.ParameterExpression.Should().Be(lambdaExpression.Parameters[0]);
        }

        [Fact]
        public async Task Updates_parent_when_logical_predicate_changes()
        {
            // Arrange
            Expression<Func<Person, bool>> lambdaExpression = person => person.PersonId == "1" && person.PersonId == "2";
            bool updated = false;
            var component = RenderComponent<BlazorQueryBuilder.Pages.Predicate>(parameters =>
            {
                parameters
                    .Add(p => p.Expression, lambdaExpression.Body as BinaryExpression)
                    .Add(p => p.ParameterExpression, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { updated = true;  });
            });

            // Act
            var logicalPredicate = component.FindComponent<LogicalPredicate>();
            await logicalPredicate.InvokeAsync(() =>
            {
                logicalPredicate.Instance.OnChange.Invoke(lambdaExpression.Body as BinaryExpression);
            });

            // Assert
            updated.Should().BeTrue();
        }

        [Fact]
        public async Task Updates_parent_when_relational_predicate_changes()
        {
            // Arrange
            Expression<Func<Person, bool>> lambdaExpression = person => person.PersonId == "1";
            bool updated = false;
            var component = RenderComponent<BlazorQueryBuilder.Pages.Predicate>(parameters =>
            {
                parameters
                    .Add(p => p.Expression, lambdaExpression.Body)
                    .Add(p => p.ParameterExpression, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { updated = true; });
            });

            // Act
            var relationalPredicate = component.FindComponent<RelationalPredicate>();
            await relationalPredicate.InvokeAsync(() =>
            {
                relationalPredicate.Instance.OnChange.Invoke(lambdaExpression.Body);
            });

            // Assert
            updated.Should().BeTrue();
        }

        public static TheoryData<Expression<Func<Address, bool>>> RelationalPredicateTestData =>
        [
            address => address.AddressId == 1,
            address => address.AddressId != 1,
            address => address.AddressId > 1,
            address => address.AddressId >= 1,
            address => address.AddressId < 1,
            address => address.AddressId <= 1,
            address => address.City.StartsWith('a')
        ];

        public static TheoryData<Expression<Func<Person, bool>>> LogicalPredicateTestData =>
        [
            person => person.PersonId == "1" && person.PersonId == "2",
            person => person.PersonId == "1" || person.PersonId == "2",
        ];
    }
}
