using BlazorQueryBuilder.Pages;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace BlazorQueryBuilder.Tests.Pages
{
    public class LogicalPredicateTests : TestContext
    {
        private Expression<Func<Person, bool>> _lambdaExpression;

        public LogicalPredicateTests()
        {
            Services.AddSingleton<PredicateFactory>();

            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;
            JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);

            RenderComponent<MudPopoverProvider>();

            _lambdaExpression = person => person.PersonId == "1" || person.PersonId == "2";
        }

        [Fact]
        public async Task Initializes_predicate()
        {
            // Arrange
            
            // Act
            var component = RenderComponent<LogicalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.Binary, _lambdaExpression.Body as BinaryExpression)
                    .Add(p => p.Parameter, _lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Assert
            component.Instance.Binary.Should().Be(_lambdaExpression.Body as BinaryExpression);
            component.Instance.Parameter.Should().Be(_lambdaExpression.Parameters[0]);
        }

        [Fact]
        public async Task Initializes_left_expression()
        {
            // Arrange
            var leftBinaryExpression = ((BinaryExpression)_lambdaExpression.Body).Left;
            
            // Act
            var component = RenderComponent<LogicalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.Binary, _lambdaExpression.Body as BinaryExpression)
                    .Add(p => p.Parameter, _lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Assert
            var leftPredicate = component.FindComponents<RelationalPredicate>()[0];
            leftPredicate.Instance.Binary.Should().Be(leftBinaryExpression);
        }

        [Fact]
        public async Task Initializes_right_expression()
        {
            // Arrange
            var rightBinaryExpression = ((BinaryExpression)_lambdaExpression.Body).Right;

            // Act
            var component = RenderComponent<LogicalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.Binary, _lambdaExpression.Body as BinaryExpression)
                    .Add(p => p.Parameter, _lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Assert
            var rightPredicate = component.FindComponents<RelationalPredicate>()[1];
            rightPredicate.Instance.Binary.Should().Be(rightBinaryExpression);
        }

        [Theory]
        [MemberData(nameof(LogicalPredicateTestData))]
        public async Task Initializes_operator(string logicalOperator, Expression<Func<Person, bool>> lambdaExpression)
        {
            // Act
            var component = RenderComponent<LogicalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.Binary, lambdaExpression.Body as BinaryExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Assert
            var logicalOperatorSelect = component
                .FindComponents<MudSelect<string>>()
                .Where(s => s.Instance.Label == "Operator")
                .ToList()[1];

            logicalOperatorSelect.Instance.Value.Should().Be(logicalOperator);
        }

        [Fact]
        public async Task Updates_left_expression()
        {
            // Arrange
            var component = RenderComponent<LogicalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.Binary, _lambdaExpression.Body as BinaryExpression)
                    .Add(p => p.Parameter, _lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Act
            var leftPredicate = component.FindComponents<RelationalPredicate>()[0];
            Expression<Func<Person, bool>> newLeftLambda = person => person.PersonId == "3";
            var newLeftBinaryExpression = newLeftLambda.Body as BinaryExpression;
            await leftPredicate.InvokeAsync(() =>
            {
                leftPredicate.Instance.OnChange.Invoke(newLeftBinaryExpression);
            });

            // Assert
            component.Instance.Binary.Left.Should().Be(newLeftBinaryExpression);
        }

        [Fact]
        public async Task Updates_right_expression()
        {
            // Arrange
            var component = RenderComponent<LogicalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.Binary, _lambdaExpression.Body as BinaryExpression)
                    .Add(p => p.Parameter, _lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Act
            var rightPredicate = component.FindComponents<RelationalPredicate>()[1];
            Expression<Func<Person, bool>> newRightLambda = person => person.PersonId == "3";
            var newRightExpression = newRightLambda.Body as BinaryExpression;
            await rightPredicate.InvokeAsync(() =>
            {
                rightPredicate.Instance.OnChange.Invoke(newRightExpression);
            });

            // Assert
            component.Instance.Binary.Right.Should().Be(newRightExpression);
        }

        [Fact]
        public async Task Removes_left_expression()
        {
            // Arrange
            BinaryExpression remainingExpression = null;
            var component = RenderComponent<LogicalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.Binary, _lambdaExpression.Body as BinaryExpression)
                    .Add(p => p.Parameter, _lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, expression => { remainingExpression = expression; });
            });

            // Act
            var leftPredicate = component.FindComponents<RelationalPredicate>()[0];
            await leftPredicate.InvokeAsync(leftPredicate.Instance.OnRemove.Invoke);
            
            // Assert
            remainingExpression.Should().Be(component.Instance.Binary.Right);
        }

        [Fact]
        public async Task Removes_right_expression()
        {
            // Arrange
            Expression<Func<Person, bool>> lambdaExpression = person => person.PersonId == "1" || person.PersonId == "2";
            BinaryExpression remainingExpression = null;
            var component = RenderComponent<LogicalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.Binary, _lambdaExpression.Body as BinaryExpression)
                    .Add(p => p.Parameter, _lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, expression => { remainingExpression = expression; });
            });

            // Act
            var rightPredicate = component.FindComponents<RelationalPredicate>()[1];
            await rightPredicate.InvokeAsync(rightPredicate.Instance.OnRemove.Invoke);

            // Assert
            remainingExpression.Should().Be(component.Instance.Binary.Left);
        }

        public static TheoryData<string, Expression<Func<Person, bool>>> LogicalPredicateTestData => new()
        {
            { "AndAlso", person => person.PersonId == "1" && person.PersonId == "2" },
            { "OrElse", person => person.PersonId == "1" || person.PersonId == "2" }
        };
    }
}
