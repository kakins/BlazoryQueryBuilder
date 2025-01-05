using BlazorQueryBuilder.ExpressionVisitors.Extensions;
using BlazorQueryBuilder.Pages;
using BlazorQueryBuilder.Tests.Util;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using Bunit;
using FluentAssertions;
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
    public class RelationalValueTests : TestContext
    {
        private readonly LambdaExpression _lambdaExpression;
        private readonly ParameterExpression _predicateParameter;
        private readonly Expression _predicateExpression;

        public RelationalValueTests()
        {
            Services.AddSingleton<PredicateFactory>();

            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;
            JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);

            RenderComponent<MudPopoverProvider>();

            _lambdaExpression = ExpressionHelpers.CreateLambda<Person>(person => person.PersonId == "1");
            _predicateParameter = _lambdaExpression.Parameters[0];
            _predicateExpression = _lambdaExpression.Body;
        }

        [Fact]
        public async Task Initializes_int_value()
        {
            // Arrange
            var lambdaExpression = ExpressionHelpers.CreateLambda<Person>(person => person.NumberOfChildren == 4);
            var predicateExpression = lambdaExpression.Body;

            // Act
            var component = CreateComponent(predicateExpression, lambdaExpression.Parameters[0]);

            // Assert
            var valueInput = component.FindInputByLabel<MudTextField<int>, int>("Value");
            valueInput.Instance.Value.Should().Be(4);
        }

        [Fact]
        public async Task Updates_right_operand_to_int_value()
        {
            // Arrange
            var lambdaExpression = ExpressionHelpers.CreateLambda<Person>(person => person.NumberOfChildren == 4);
            var predicateExpression = lambdaExpression.Body.As<BinaryExpression>();
            var onValueChanged = new Mock<Action<Expression>>();
            
            var component = CreateComponent(
                predicateExpression,
                lambdaExpression.Parameters[0],
                onValueChanged.Object);

            // Act
            var valueInput = component.FindInputByLabel<MudTextField<int>, int>("Value");
            await component.InvokeAsync(() =>
            {
                valueInput.Instance.SetText("5");
            });

            // Assert
            onValueChanged.Verify(change => change(It.Is<BinaryExpression>(e => e.Right.As<ConstantExpression>().Value.As<int>() == 5)));
        }

        [Fact]
        public async Task Initializes_string_value()
        {
            // Arrange
            var lambdaExpression = ExpressionHelpers.CreateLambda<Person>(person => person.PersonId == "1");
            var predicateExpression = lambdaExpression.Body;

            // Act
            var component = CreateComponent(predicateExpression, lambdaExpression.Parameters[0]);

            // Assert
            var valueInput = component.FindInputByLabel<MudTextField<string>, string>("Value");
            valueInput.Instance.Value.Should().Be("1");
        }

        [Fact]
        public async Task Updates_right_operand_to_string_value()
        {
            // Arrange
            var lambdaExpression = ExpressionHelpers.CreateLambda<Person>(person => person.PersonId == "1");
            var predicateExpression = lambdaExpression.Body.As<BinaryExpression>();
            var onValueChanged = new Mock<Action<Expression>>();

            var component = CreateComponent(
                predicateExpression,
                lambdaExpression.Parameters[0],
                onValueChanged: onValueChanged.Object);

            // Act
            var valueInput = component.FindInputByLabel<MudTextField<string>, string>("Value");
            await component.InvokeAsync(() =>
            {
                valueInput.Instance.SetText("2");
            });

            // Assert
            onValueChanged.Verify(change => change(It.Is<BinaryExpression>(e => e.Right.As<ConstantExpression>().Value.As<string>() == "2")));
        }

        [Fact]
        public async Task Initializes_date_value()
        {
            // Arrange
            var lambdaExpression = ExpressionHelpers.CreateLambda<Person>(person => person.Created == DateTime.MinValue);
            var predicateExpression = lambdaExpression.Body.As<BinaryExpression>().ReplaceRight(Expression.Constant(DateTime.Now));

            // Act
            var component = CreateComponent(predicateExpression, lambdaExpression.Parameters[0]);

            // Assert
            var valueInput = component
                .FindComponents<MudDatePicker>()
                .FirstOrDefault(p => p.Instance.Label == "Value"); ;
            valueInput.Instance.Date.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task Updates_right_operand_to_date_value()
        {
            // Arrange
            var lambdaExpression = ExpressionHelpers.CreateLambda<Person>(person => person.Created == DateTime.MinValue);
            var predicateExpression = lambdaExpression.Body.As<BinaryExpression>().ReplaceRight(Expression.Constant(DateTime.Now));
            var onValueChanged = new Mock<Action<Expression>>();

            var component = CreateComponent(
                predicateExpression,
                lambdaExpression.Parameters[0],
                onValueChanged: onValueChanged.Object);

            // Act
            var dateTime = DateTime.Now.AddDays(1);
            var dateValueInput = component
                .FindComponents<MudDatePicker>()
                .FirstOrDefault(p => p.Instance.Label == "Value");
            await component.InvokeAsync(async () =>
            {
                await dateValueInput.Instance.DateChanged.InvokeAsync(dateTime);
            });

            // Assert
            onValueChanged.Verify(change => change(It.Is<BinaryExpression>(e => e.Right.As<ConstantExpression>().Value.As<System.DateTime>() == dateTime)));
        }

        [Fact]
        public async Task Initializes_bool_value()
        {
            // Arrange
            var lambdaExpression = ExpressionHelpers.CreateLambda<Person>(person => person.IsAlive == true);
            var predicateExpression = lambdaExpression.Body;

            // Act
            var component = CreateComponent(predicateExpression, lambdaExpression.Parameters[0]);

            // Assert
            var valueInput = component.FindComponents<MudSelect<bool>>().FirstOrDefault(c => c.Instance.Label == "Value"); ;
            valueInput.Instance.Value.Should().BeTrue();
        }

        [Fact]
        public async Task Updates_right_operand_to_bool_value()
        {
            // Arrange
            var lambdaExpression = ExpressionHelpers.CreateLambda<Person>(person => person.IsAlive == true);
            var predicateExpression = lambdaExpression.Body.As<BinaryExpression>();
            var onValueChanged = new Mock<Action<Expression>>();

            var component = CreateComponent(
                predicateExpression,
                lambdaExpression.Parameters[0],
                onValueChanged: onValueChanged.Object);

            // Act
            var valueInput = component
                .FindComponents<MudSelect<bool>>()
                .FirstOrDefault(c => c.Instance.Label == "Value");

            await component.InvokeAsync(async () =>
            {
                await valueInput.Instance.ValueChanged.InvokeAsync(false);
            });

            // Assert
            onValueChanged.Verify(change => change(It.Is<BinaryExpression>(e => e.Right.As<ConstantExpression>().Value.As<bool>() == false)));
        }

        private IRenderedComponent<RelationalValue> CreateComponent(
            Expression predicateExpression = null,
            ParameterExpression parameterExpression = null,
            Action<Expression> onValueChanged = null)
        {
            onValueChanged ??= (_) => { };

            return RenderComponent<RelationalValue>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, predicateExpression ?? _predicateExpression)
                    .Add(p => p.ParameterExpression, parameterExpression ?? _predicateParameter)
                    .Add(p => p.OnValueChanged, onValueChanged);
            });
        }
    }
}
