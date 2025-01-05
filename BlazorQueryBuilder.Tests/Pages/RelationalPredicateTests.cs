using BlazorQueryBuilder.ExpressionVisitors.Extensions;
using BlazorQueryBuilder.Pages;
using BlazorQueryBuilder.Tests.Util;
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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace BlazorQueryBuilder.Tests.Pages
{
    public class RelationalPredicateTests : TestContext
    {
        private readonly LambdaExpression _lambdaExpression;
        private readonly ParameterExpression _predicateParameter;
        private readonly Expression _predicateExpression;

        public RelationalPredicateTests()
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

        [Theory]
        [MemberData(nameof(FieldItemTestData))]
        public async Task Initializes_field_items(LambdaExpression lambdaExpression, string expectedFieldName)
        {
            // Arrange
            var component = CreateComponent(lambdaExpression.Body, lambdaExpression.Parameters[0]);

            // Act
            var field = component.FindComponent<RelationalPredicateField>();

            // Assert
            field.Instance.PredicateExpression.Should().Be(lambdaExpression.Body);
            field.Instance.ParameterExpression.Should().Be(lambdaExpression.Parameters[0]);
        }

        [Fact]
        public async Task Updates_operators_on_field_change()
        {
            // Arrange
            var lambdaExpression = ExpressionHelpers.CreateLambda<Person>(person => person.PersonId == "1");
            var component = CreateComponent(
                lambdaExpression.Body, 
                lambdaExpression.Parameters[0]);

            // Act
            var updatedExpression = ExpressionHelpers.CreateLambda<Person>(person => person.PersonId != "1").Body;
            var field = component.FindComponent<RelationalPredicateField>();
            await component.InvokeAsync(() =>
            {
                field.Instance.OnFieldChanged.Invoke(updatedExpression);
            });

            // Assert
            var operators = component.FindComponent<RelationalOperators>();
            operators.Instance.PredicateExpression.Should().Be(updatedExpression);
        }

        [Fact]
        public async Task Updates_expression_on_field_change()
        {
            // Arrange
            var lambdaExpression = ExpressionHelpers.CreateLambda<Person>(person => person.PersonId == "1");
            var component = CreateComponent(
                lambdaExpression.Body,
                lambdaExpression.Parameters[0]);

            // Act
            var updatedExpression = ExpressionHelpers.CreateLambda<Person>(person => person.PersonId != "1").Body;
            var field = component.FindComponent<RelationalPredicateField>();
            await component.InvokeAsync(() =>
            {
                field.Instance.OnFieldChanged.Invoke(updatedExpression);
            });

            // Assert
            component.Instance.PredicateExpression.Should().Be(updatedExpression);
        }

        [Fact]
        public async Task Updates_navigation_path_on_field_change()
        {
            // Arrange
            var lambdaExpression = ExpressionHelpers.CreateLambda<Address>(address => address.PersonId == "1");
            var component = CreateComponent(
                lambdaExpression.Body,
                lambdaExpression.Parameters[0]);

            // Act
            var field = component.FindComponent<RelationalPredicateField>();
            await component.InvokeAsync(() =>
            {
                field.Instance.OnNavigatePathChanged.Invoke("Person");
            });
            var navigationPathChips = component
                .FindComponent<MudChipSet<string>>()
                .FindComponents<MudChip<string>>();

            // Assert
            navigationPathChips
                .Select(c => c.Instance.Text)
                .Should()
                .BeEquivalentTo(["Address", "Person"]);
        }

        [Theory]
        [MemberData(nameof(PredicateOperatorTestData))]
        public async Task Initializes_operator(Expression<Func<Person, bool>> lambdaExpression)
        {
            // Arrange
            var predicateExpression = lambdaExpression.Body;
            var predicateParameter = lambdaExpression.Parameters[0];
            
            var component = CreateComponent(predicateExpression, predicateParameter);   

            // Act
            var operators = component.FindComponent<RelationalOperators>();

            // Assert
            operators.Instance.PredicateExpression.Should().Be(predicateExpression);
        }

        [Fact]
        public async Task Updates_expression_when_operator_changes()
        {
            // Arrange
            var predicateExpression = _predicateExpression;
            var onChange = new Mock<Action<Expression>>();
            var component = CreateComponent(
                _predicateExpression, 
                _predicateParameter,
                onChange: onChange.Object);

            // Act
            var updatedExpression = ExpressionHelpers.CreateLambda<Person>(person => person.PersonId != "1").Body;
            var operators = component.FindComponent<RelationalOperators>();
            await component.InvokeAsync(() =>
            {
                operators.Instance.OnOperatedUpdated(updatedExpression);
            });

            // Assert
            onChange.Verify(o => o.Invoke(updatedExpression), Times.Once);
        }

        [Fact]
        public async Task Updates_operators_on_value_change()
        {
            // Arrange
            var lambdaExpression = ExpressionHelpers.CreateLambda<Person>(person => person.PersonId == "1");
            var component = CreateComponent(
                lambdaExpression.Body,
                lambdaExpression.Parameters[0]);

            // Act
            var updatedExpression = lambdaExpression.Body
                .As<BinaryExpression>()
                .ReplaceRight(Expression.Constant("2"));
            var value = component.FindComponent<RelationalValue>();
            await component.InvokeAsync(() =>
            {
                value.Instance.OnValueChanged(updatedExpression);
            });

            // Assert
            component
                .FindComponent<RelationalOperators>()
                .Instance
                .PredicateExpression
                .Should()
                .BeEquivalentTo(updatedExpression);
        }

        [Theory]
        [MemberData(nameof(AddPredicateData))]
        public async Task Adds_predicate_expression(string buttonText, LambdaExpression lambdaExpression)
        {
            // Arrange
            var originalPredicateExpression = lambdaExpression.Body;
            BinaryExpression updatedPredicateExpression = null;

            var component = CreateComponent(
                originalPredicateExpression,
                lambdaExpression.Parameters[0],
                updatedExpression => { updatedPredicateExpression = updatedExpression as BinaryExpression; });

            // Act
            component
                .FindComponents<MudButton>()
                .Single(button => button.Markup.Contains(buttonText))
                .Find("button")
                .Click();

            // Assert
            updatedPredicateExpression.Should().NotBeNull();
            updatedPredicateExpression.Should().NotBe(originalPredicateExpression);
            updatedPredicateExpression.Left.Should().Be(originalPredicateExpression);
            updatedPredicateExpression.NodeType.Should().Be(buttonText == "And" ? ExpressionType.AndAlso : ExpressionType.OrElse);
            // TODO: Add better assertion for right operand
            updatedPredicateExpression.Right.Should().NotBeNull();
        }

        public static TheoryData<string, Expression<Func<Address, bool>>> AddPredicateData()
        {
            return new()
            {
                { "And", address => address.PersonId == "1" },
                { "And", address => EF.Functions.Like(address.City, "%Alice%") },
                { "Or", address => address.PersonId == "1" },
                { "Or", address => EF.Functions.Like(address.City, "%Alice%") }
            };
        }

        [Fact]
        public async Task Removes_predicate_expression()
        {
            // Arrange
            var lambdaExpression = ExpressionHelpers.CreateLambda<Person>(person => person.PersonId == "1");
            var originalPredicateExpression = lambdaExpression.Body;
            var removed = false;

            var component = CreateComponent(
                originalPredicateExpression,
                lambdaExpression.Parameters[0],
                onRemove: () => { removed = true; });

            // Act
            await component.InvokeAsync(() =>
            {
                component.Instance.OnRemove.Invoke();
            });

            // Assert
            removed.Should().BeTrue();
        }

        private IRenderedComponent<RelationalPredicate> CreateComponent(
            Expression predicateExpression = null, 
            ParameterExpression parameterExpression = null,
            Action<Expression> onChange = null,
            Action onRemove = null)
        {
            onChange ??= (_) => { };
            onRemove ??= () => { };

            return RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, predicateExpression ?? _predicateExpression)
                    .Add(p => p.ParameterExpression, parameterExpression ?? _predicateParameter)
                    .Add(p => p.OnChange, onChange)
                    .Add(p => p.OnRemove, onRemove);
            });
        }
        
        public static TheoryData<List<string>> LeftOperandTestData() => 
        [
            // address.PersonId
            [nameof(Address.PersonId)],
            // address.Person.LastName
            [nameof(Address.Person), nameof(Person.LastName)],
            // address.Utilities.Count
            [nameof(Address.Utilities), nameof(List<Utility>.Count)]
        ];

        public static TheoryData<LambdaExpression, string> FieldItemTestData()
        {
            Expression<Func<Person, bool>> binaryLambda = person => person.PersonId == "1";
            Expression<Func<Person, bool>> methodCallLambda = p => EF.Functions.Like(p.FirstName, "%Alice%");

            return new()
            {
                { binaryLambda, nameof(Person.PersonId) },
                { methodCallLambda, nameof(Person.FirstName) }
            };
        }

        public static TheoryData<Expression<Func<Person, bool>>> PredicateOperatorTestData =>
        [
            person => person.PersonId == "1",
            person => person.PersonId != "1",
            person => person.NumberOfChildren == 4,
            person => person.NumberOfChildren != 4,
            person => person.NumberOfChildren > 4,
            person => person.NumberOfChildren >= 4,
            person => person.NumberOfChildren < 4,
            person => person.NumberOfChildren <= 4,
            person => person.Created == DateTime.Now,
            person => person.Created != DateTime.Now,
            person => person.Created > DateTime.Now,
            person => person.Created >= DateTime.Now,
            person => person.Created < DateTime.Now,
            person => person.Created <= DateTime.Now,
            person => person.IsAlive == true,
            person => person.IsAlive != true,
            person => EF.Functions.Like(person.FirstName, "%Alice%"),
        ];
    }
}
