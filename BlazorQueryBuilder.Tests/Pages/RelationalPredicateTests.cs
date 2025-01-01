using BlazorQueryBuilder.ExpressionVisitors.Extensions;
using BlazorQueryBuilder.Pages;
using BlazorQueryBuilder.Tests.Util;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using BlazoryQueryBuilder.Shared.Util;
using Bunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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

            _lambdaExpression = GetLambdaExpression<Person>(person => person.PersonId == "1");
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
            var lambdaExpression = GetLambdaExpression<Person>(person => person.PersonId == "1");
            var component = CreateComponent(
                lambdaExpression.Body, 
                lambdaExpression.Parameters[0]);

            // Act
            var updatedExpression = GetLambdaExpression<Person>(person => person.PersonId != "1").Body;
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
            var lambdaExpression = GetLambdaExpression<Person>(person => person.PersonId == "1");
            var component = CreateComponent(
                lambdaExpression.Body,
                lambdaExpression.Parameters[0]);

            // Act
            var updatedExpression = GetLambdaExpression<Person>(person => person.PersonId != "1").Body;
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
            var lambdaExpression = GetLambdaExpression<Address>(person => person.PersonId == "1");
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
            var component = CreateComponent(
                _predicateExpression, 
                _predicateParameter,
                onChange: expression => { predicateExpression = expression; });

            // Act
            var updatedExpression = GetLambdaExpression<Person>(person => person.PersonId != "1").Body;
            var operators = component.FindComponent<RelationalOperators>();
            await component.InvokeAsync(() =>
            {
                operators.Instance.OnChange(updatedExpression);
            });

            // Assert
            predicateExpression.Should().BeEquivalentTo(updatedExpression);
        }

        [Fact]
        public async Task Initializes_int_value()
        {
            // Arrange
            var lambdaExpression = GetLambdaExpression<Person>(person => person.NumberOfChildren == 4);
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
            var lambdaExpression = GetLambdaExpression<Person>(person => person.NumberOfChildren == 4);
            var predicateExpression = GetLambdaBodyExpression<BinaryExpression>(lambdaExpression);

            var component = CreateComponent(
                predicateExpression, 
                lambdaExpression.Parameters[0], 
                updatedExpression => { predicateExpression = updatedExpression as BinaryExpression; });

            // Act
            var valueInput = component.FindInputByLabel<MudTextField<int>, int>("Value");
            await component.InvokeAsync(() =>
            {
                valueInput.Instance.SetText("5");
            });

            // Assert
            var rightOperand = predicateExpression.Right as ConstantExpression;
            rightOperand.Value.Should().Be(5);
        }

        [Fact]
        public async Task Initializes_string_value()
        {
            // Arrange
            var lambdaExpression = GetLambdaExpression<Person>(person => person.PersonId == "1");
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
            var lambdaExpression = GetLambdaExpression<Person>(person => person.PersonId == "1");
            var predicateExpression = GetLambdaBodyExpression<BinaryExpression>(lambdaExpression);

            var component = CreateComponent(
                predicateExpression,
                lambdaExpression.Parameters[0],
                onChange: updatedExpression => { predicateExpression = updatedExpression as BinaryExpression; });

            // Act
            var valueInput = component.FindInputByLabel<MudTextField<string>, string>("Value");
            await component.InvokeAsync(() =>
            {
                valueInput.Instance.SetText("2");
            });

            // Assert
            var rightOperand = predicateExpression.Right as ConstantExpression;
            rightOperand.Value.Should().Be("2");
        }

        [Fact]
        public async Task Initializes_date_value()
        {
            // Arrange
            var lambdaExpression = GetLambdaExpression<Person>(person => person.Created == DateTime.MinValue);
            var predicateExpression = GetLambdaBodyExpression<BinaryExpression>(lambdaExpression).ReplaceRight(DateTimeExpression.New(DateTime.Now));

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
            var lambdaExpression = GetLambdaExpression<Person>(person => person.Created == DateTime.MinValue);
            var predicateExpression = GetLambdaBodyExpression<BinaryExpression>(lambdaExpression).ReplaceRight(DateTimeExpression.New(DateTime.Now));

            var component = CreateComponent(
                predicateExpression, 
                lambdaExpression.Parameters[0],
                onChange: updatedExpression => { predicateExpression = updatedExpression as BinaryExpression; });

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
            var rightOperand = predicateExpression.Right as NewExpression;
            var rightOperandConstant = rightOperand.Arguments[0] as ConstantExpression;
            rightOperandConstant.Value.Should().Be(dateTime.Ticks);
        }

        [Fact]
        public async Task Initializes_bool_value()
        {
            // Arrange
            var lambdaExpression = GetLambdaExpression<Person>(person => person.IsAlive == true);
            var predicateExpression = lambdaExpression.Body;

            // Act
            var component = CreateComponent(predicateExpression, lambdaExpression.Parameters[0]);   

            // Assert
            var valueInput = component.FindComponents<MudCheckBox<bool>>().FirstOrDefault(c => c.Instance.Label == "Value"); ;
            valueInput.Instance.Value.Should().BeTrue();
        }

        [Fact]
        public async Task Updates_right_operand_to_bool_value()
        {
            // Arrange
            var lambdaExpression = GetLambdaExpression<Person>(person => person.IsAlive == true);
            var predicateExpression = GetLambdaBodyExpression<BinaryExpression>(lambdaExpression);

            var component = CreateComponent(
                predicateExpression, 
                lambdaExpression.Parameters[0], 
                onChange: updatedExpression => { predicateExpression = updatedExpression as BinaryExpression; });

            // Act
            var valueInput = component
                .FindComponents<MudCheckBox<bool>>()
                .FirstOrDefault(c => c.Instance.Label == "Value");
            
            await component.InvokeAsync(async () =>
            {
                await valueInput.Instance.ValueChanged.InvokeAsync(false);
            });

            // Assert
            var rightOperand = predicateExpression.Right as ConstantExpression;
            rightOperand.Value.Should().Be(false);
        }

        [Theory]
        [InlineData("And")]
        [InlineData("Or")]
        public async Task Adds_predicate_expression(string buttonText)
        {
            // Arrange
            var lambdaExpression = GetLambdaExpression<Person>(person => person.PersonId == "1");
            var originalPredicateExpression = GetLambdaBodyExpression<BinaryExpression>(lambdaExpression);
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

        [Fact]
        public async Task Removes_predicate_expression()
        {
            // Arrange
            var lambdaExpression = GetLambdaExpression<Person>(person => person.PersonId == "1");
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

        private LambdaExpression GetLambdaExpression<T>(Expression<Func<T, bool>> lambdaExpression) => lambdaExpression;

        private T GetLambdaBodyExpression<T>(LambdaExpression lambdaExpression) where T : Expression => lambdaExpression.Body as T;
        
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
