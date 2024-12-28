using BlazorQueryBuilder.ExpressionVisitors;
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

        [Fact]
        public async Task Initializes_predicate()
        {
            // Arrange
            Expression<Func<Person, bool>> lambdaExpression = person => person.PersonId == "1";
            
            // Act
            var component = RenderComponent<LambdaComponent>(parameters =>
            {
                parameters
                    .Add(p => p.Lambda, lambdaExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChanged, _ => { });
            });

            // Assert
            var predicateComponent = component.FindComponent<BlazorQueryBuilder.Pages.Predicate>();
            predicateComponent.Instance.Expression.Should().Be(lambdaExpression.Body as BinaryExpression);
        }

        [Fact]
        public async Task Updates_parent_component_when_lambda_body_changes()
        {
            // Arrange
            LambdaExpression lambdaExpression = (Expression<Func<Person, bool>>)(person => person.PersonId == "1");
            var component = RenderComponent<LambdaComponent>(parameters =>
            {
                parameters
                    .Add(p => p.Lambda, lambdaExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChanged, expression => 
                    {
                        lambdaExpression = lambdaExpression.ReplaceBody(expression as BinaryExpression); 
                    });
            });

            // Act
            var predicateComponent = component.FindComponent<BlazorQueryBuilder.Pages.Predicate>();
            Expression<Func<Person, bool>> updatedLambdaExpression = person => person.PersonId == "2";
            await predicateComponent.InvokeAsync(() =>
            {
                predicateComponent.Instance.OnChange.Invoke(updatedLambdaExpression.Body as BinaryExpression);
            });

            // Assert
            lambdaExpression.Should().BeEquivalentTo(updatedLambdaExpression);
        }

        [Fact]
        public async Task Displays_lambda_expression()
        {
            // Arrange
            Expression<Func<Person, bool>> lambdaExpression = person => person.PersonId == "1";

            // Act
            var component = RenderComponent<LambdaComponent>(parameters =>
            {
                parameters
                    .Add(p => p.Lambda, lambdaExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChanged, _ => { });
            });

            // Assert
            var expansionPanel = component
                .FindComponents<MudExpansionPanel>()
                .FirstOrDefault(panel => panel.Instance.Text == "View Lambda Expression");
            expansionPanel
                .Find("code")
                .TextContent
                .Should()
                .Be(lambdaExpression.ToString());
        }
    }
}
