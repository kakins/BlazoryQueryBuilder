using BlazorQueryBuilder.Pages;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using Bunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
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

        [Theory]
        [MemberData(nameof(LogicalPredicateData))]
        public async Task Initializes_predicate(LambdaExpression lambdaExpression)
        {
            // Arrange
            
            // Act
            var component = RenderComponent<LogicalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, lambdaExpression.Body as BinaryExpression)
                    .Add(p => p.ParameterExpression, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Assert
            component.Instance.PredicateExpression.Should().BeEquivalentTo(lambdaExpression.Body as BinaryExpression);
            component.Instance.ParameterExpression.Should().BeEquivalentTo(lambdaExpression.Parameters[0]);
        }

        [Theory]
        [MemberData(nameof(LogicalPredicateData))]
        public async Task Initializes_lefts_expression(LambdaExpression lambdaExpression)
        {
            // Arrange
            var leftBinaryExpression = ((BinaryExpression)lambdaExpression.Body).Left;
            
            // Act
            var component = RenderComponent<LogicalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, lambdaExpression.Body as BinaryExpression)
                    .Add(p => p.ParameterExpression, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Assert
            var leftPredicate = component.FindComponents<RelationalPredicate>()[0];
            leftPredicate.Instance.PredicateExpression.Should().Be(leftBinaryExpression);
        }

        [Theory]
        [MemberData(nameof(LogicalPredicateData))]
        public async Task Initializes_right_expression(LambdaExpression lambdaExpression)
        {
            // Arrange
            var rightBinaryExpression = ((BinaryExpression)lambdaExpression.Body).Right;

            // Act
            var component = RenderComponent<LogicalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, lambdaExpression.Body as BinaryExpression)
                    .Add(p => p.ParameterExpression, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Assert
            var rightPredicate = component.FindComponents<RelationalPredicate>()[1];
            rightPredicate.Instance.PredicateExpression.Should().Be(rightBinaryExpression);
        }

        [Theory]
        [MemberData(nameof(LogicalPredicateOperatorData))]
        public async Task Initializes_operator(string logicalOperator, Expression<Func<Person, bool>> lambdaExpression)
        {
            // Act
            var component = RenderComponent<LogicalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, lambdaExpression.Body as BinaryExpression)
                    .Add(p => p.ParameterExpression, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Assert
            var logicalOperatorSelect = component
                .FindComponents<MudSelect<string>>()
                .Where(s => s.Instance.Label == "Operator")
                .ToList()[0];

            logicalOperatorSelect.Instance.Value.Should().Be(logicalOperator);
        }

        [Theory]
        [MemberData(nameof(LogicalPredicateData))]
        public async Task Updates_left_expression(LambdaExpression lambdaExpression)
        {
            // Arrange
            var component = RenderComponent<LogicalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, lambdaExpression.Body as BinaryExpression)
                    .Add(p => p.ParameterExpression, lambdaExpression.Parameters[0])
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
            component.Instance.PredicateExpression.Left.Should().Be(newLeftBinaryExpression);
        }

        [Theory]
        [MemberData(nameof(LogicalPredicateData))]
        public async Task Updates_right_expression(LambdaExpression lambdaExpression)
        {
            // Arrange
            var component = RenderComponent<LogicalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, lambdaExpression.Body as BinaryExpression)
                    .Add(p => p.ParameterExpression, lambdaExpression.Parameters[0])
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
            component.Instance.PredicateExpression.Right.Should().Be(newRightExpression);
        }

        [Theory]
        [MemberData(nameof(LogicalPredicateData))]
        public async Task Removes_left_expression(LambdaExpression lambdaExpression)
        {
            // Arrange
            var onChange = new Mock<Action<Expression>>();
            var component = RenderComponent<LogicalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, lambdaExpression.Body as BinaryExpression)
                    .Add(p => p.ParameterExpression, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, onChange.Object );
            });

            // Act
            var leftPredicate = component.FindComponents<RelationalPredicate>()[0];
            await leftPredicate.InvokeAsync(leftPredicate.Instance.OnRemove.Invoke);

            // Assert
            onChange.Verify(o => o.Invoke(component.Instance.PredicateExpression.Right), Times.Once);
        }

        [Theory]
        [MemberData(nameof(LogicalPredicateData))]
        public async Task Removes_right_expression(LambdaExpression lambdaExpression)
        {
            // Arrange
            var onChange = new Mock<Action<Expression>>();
            var component = RenderComponent<LogicalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, lambdaExpression.Body as BinaryExpression)
                    .Add(p => p.ParameterExpression, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, onChange.Object);
            });

            // Act
            var rightPredicate = component.FindComponents<RelationalPredicate>()[1];
            await rightPredicate.InvokeAsync(rightPredicate.Instance.OnRemove.Invoke);

            // Assert
            onChange.Verify(o => o.Invoke(component.Instance.PredicateExpression.Left), Times.Once);
        }

        public static TheoryData<string, Expression<Func<Person, bool>>> LogicalPredicateOperatorData => new()
        {
            { "AndAlso", person => person.PersonId == "1" && person.PersonId == "2" },
            { "OrElse", person => person.PersonId == "1" || person.PersonId == "2" }
        };

        public static TheoryData<Expression<Func<Person, bool>>> LogicalPredicateData => new()
        {
            { person => person.PersonId == "1" && person.PersonId == "2" },
            { person => person.PersonId == "1" && EF.Functions.Like(person.LastName, "%Alice%") },
            { person => EF.Functions.Like(person.LastName, "%Alice%") && person.PersonId == "1" },
        };
    }
}
