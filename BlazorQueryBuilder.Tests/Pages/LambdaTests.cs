using BlazorQueryBuilder.ExpressionVisitors.Extensions;
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
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace BlazorQueryBuilder.Tests.Pages
{
    public class LambdaTests : TestContext
    {
        public LambdaTests()
        {
            Services.AddSingleton<PredicateFactory>();
            
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;
            JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);

            RenderComponent<MudPopoverProvider>();
        }

        [Theory]
        [MemberData(nameof(LambdaExpressionData))]
        public async Task Initializes_predicate(Expression<Func<Person, bool>> lambdaExpression)
        {
            // Arrange            
            // Act
            var component = RenderComponent<LambdaComponent>(parameters =>
            {
                parameters
                    .Add(p => p.Lambda, lambdaExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChanged, _ => { });
            });

            // Assert
            component
                .FindComponent<BlazorQueryBuilder.Pages.Predicate>()
                .Instance
                .PredicateExpression
                .Should()
                .Be(lambdaExpression.Body);
        }

        [Theory]
        [MemberData(nameof(LambdaExpressionData))]
        public async Task Updates_parent_component_when_lambda_body_changes(LambdaExpression lambdaExpression)
        {
            // Arrange
            var onChanged = new Mock<Action<Expression>>();
            var component = RenderComponent<LambdaComponent>(parameters =>
            {
                parameters
                    .Add(p => p.Lambda, lambdaExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChanged, onChanged.Object);
            });

            // Act
            var predicateComponent = component.FindComponent<BlazorQueryBuilder.Pages.Predicate>();
            Expression<Func<Person, bool>> updatedLambdaExpression = person => person.PersonId == "2";
            await predicateComponent.InvokeAsync(() =>
            {
                predicateComponent.Instance.OnChange.Invoke(updatedLambdaExpression.Body);
            });

            // Assert
            onChanged.Verify(o => o.Invoke(updatedLambdaExpression.Body), Times.Once);
        }

        [Theory]
        [MemberData(nameof(LambdaExpressionData))]
        public async Task Displays_lambda_expression(LambdaExpression lambdaExpression)
        {
            // Arrange
            // Act
            var component = RenderComponent<LambdaComponent>(parameters =>
            {
                parameters
                    .Add(p => p.Lambda, lambdaExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChanged, _ => { });
            });

            // Assert
            var card = component
                .FindComponent<MudCard>();
            card
                .Find("code")
                .TextContent
                .Should()
                .Be(lambdaExpression.ToString());
        }

        public static TheoryData<Expression<Func<Person, bool>>> LambdaExpressionData =>
        [
            person => person.PersonId == "1",
            person => EF.Functions.Like(person.LastName, "Doe"),
        ];
    }
}
