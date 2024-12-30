using BlazorQueryBuilder.ExpressionVisitors.Extensions;
using BlazorQueryBuilder.Pages;
using BlazorQueryBuilder.Tests.Util;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using BlazoryQueryBuilder.Shared.Util;
using Bunit;
using FluentAssertions;
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
        private readonly BinaryExpression _predicateExpression;

        public RelationalPredicateTests()
        {
            Services.AddSingleton<PredicateFactory>();

            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;
            JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);

            RenderComponent<MudPopoverProvider>();

            _lambdaExpression = GetLambdaExpression<Person>(person => person.PersonId == "1");
            _predicateParameter = _lambdaExpression.Parameters[0];
            _predicateExpression = GetLambdaBodyExpression(_lambdaExpression);
        }

        [Fact]
        public async Task Initializes_field_items()
        {
            // Arrange
            var component = CreateComponent(_predicateExpression, _predicateParameter);

            // Act
            var fieldSelect = component.FindInputByLabel<MudSelect<string>, string>("Field");
            var fieldItems = fieldSelect.Instance.Items.ToList();
            await component.InvokeAsync(async () =>
            {
                await fieldSelect.Instance.OpenMenu();
                fieldItems = [.. fieldSelect.Instance.Items];
            });

            // Assert
            var expectedFieldName = ((MemberExpression)component.Instance.PredicateExpression.Left).Member.Name;
            fieldSelect.Instance.Value.Should().Be(expectedFieldName);

            var expectedFieldItems = _lambdaExpression.Parameters[0].Type.GetProperties().Select(p => p.Name);
            fieldItems
                .Select(i => i.Value)
                .Should()
                .BeEquivalentTo(expectedFieldItems);
        }

        [Theory]
        [MemberData(nameof(LeftOperandTestData))]
        public async Task Updates_left_operand_to_selected_field(List<string> fields)
        {
            // Arrange
            var lambdaExpression = GetLambdaExpression<Address>(address => address.AddressId == 1);
            var predicateExpression = GetLambdaBodyExpression(lambdaExpression);
            var predicateParameter = lambdaExpression.Parameters[0];

            var component = CreateComponent(
                predicateExpression,
                predicateParameter,
                onChange: expression => { predicateExpression = expression; });

            // Act
            var fieldSelect = component.FindInputByLabel<MudSelect<string>, string>("Field");

            await component.InvokeAsync(async () =>
            {
                foreach (var property in fields)
                {
                    await fieldSelect.Instance.OpenMenu();
                    await fieldSelect.Instance.SelectOption(property);
                }
            });

            // Assert
            // Verify the operand member expression matches the property selections
            var leftOperandMembers = ((MemberExpression)predicateExpression.Left).GetMemberExpressionMembers();
            leftOperandMembers.Should().BeEquivalentTo(fields);
            fieldSelect.Instance.Value.Should().Be(fields.Last());
        }

        [Fact]
        public async Task Updates_left_operand_to_navigation_field_first_property()
        {
            // Arrange
            var lambdaExpression = GetLambdaExpression<Address>(address => address.AddressId == 1);
            var predicateExpression = GetLambdaBodyExpression(lambdaExpression);
            var predicateParameter = lambdaExpression.Parameters[0];

            var component = CreateComponent(
                predicateExpression, 
                predicateParameter,
                onChange: expression => { predicateExpression = expression; });

            // Act
            var fieldSelect = component.FindInputByLabel<MudSelect<string>, string>("Field");
            await component.InvokeAsync(async () =>
            {
                await fieldSelect.Instance.OpenMenu();
                await fieldSelect.Instance.SelectOption(nameof(Address.Person));
            });

            // Assert
            var leftOperand = ((MemberExpression)predicateExpression.Left);
            leftOperand.Member.DeclaringType.Should().Be(typeof(Person));   
            leftOperand.Member.Name.Should().Be(nameof(Address.Person.PersonId));
            fieldSelect.Instance.Value.Should().Be(nameof(Address.Person.PersonId));
        }

        [Theory]
        [MemberData(nameof(PredicateOperatorTestData))]
        public async Task Initializes_operator(Expression<Func<Person, bool>> lambdaExpression)
        {
            // Arrange
            var predicateExpression = GetLambdaBodyExpression(lambdaExpression);
            var predicateParameter = lambdaExpression.Parameters[0];
            
            var component = CreateComponent(predicateExpression, predicateParameter);   

            // Act
            var operators = component.FindComponent<RelationalOperators>();

            // Assert
            operators.Instance.ExpressionType.Should().Be(predicateExpression.NodeType);
        }

        [Fact]
        public async Task Updates_operator_to_selected_operator()
        {
            // Arrange
            var predicateExpression = _predicateExpression;
            var component = CreateComponent(
                _predicateExpression, 
                _predicateParameter,
                onChange: updatedExpression => { predicateExpression = updatedExpression; });

            // Act
            var operators = component.FindComponent<RelationalOperators>();
            await component.InvokeAsync(() =>
            {
                operators.Instance.OnChange(ExpressionType.NotEqual);
            });

            // Assert
            predicateExpression.NodeType.Should().Be(ExpressionType.NotEqual);
        }

        [Fact]
        public async Task Initializes_int_value()
        {
            // Arrange
            var lambdaExpression = GetLambdaExpression<Person>(person => person.NumberOfChildren == 4);
            var predicateExpression = GetLambdaBodyExpression(lambdaExpression);

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
            var predicateExpression = GetLambdaBodyExpression(lambdaExpression);

            var component = CreateComponent(
                predicateExpression, 
                lambdaExpression.Parameters[0], 
                updatedExpression => { predicateExpression = updatedExpression; });

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
            var predicateExpression = GetLambdaBodyExpression(lambdaExpression);

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
            var predicateExpression = GetLambdaBodyExpression(lambdaExpression);

            var component = CreateComponent(
                predicateExpression,
                lambdaExpression.Parameters[0],
                onChange: updatedExpression => { predicateExpression = updatedExpression; });

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
            var predicateExpression = GetLambdaBodyExpression(lambdaExpression).ReplaceRight(DateTimeExpression.New(DateTime.Now));

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
            var predicateExpression = GetLambdaBodyExpression(lambdaExpression).ReplaceRight(DateTimeExpression.New(DateTime.Now));

            var component = CreateComponent(
                predicateExpression, 
                lambdaExpression.Parameters[0],
                onChange: updatedExpression => { predicateExpression = updatedExpression; });

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
            var predicateExpression = GetLambdaBodyExpression(lambdaExpression);

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
            var predicateExpression = GetLambdaBodyExpression(lambdaExpression);

            var component = CreateComponent(
                predicateExpression, 
                lambdaExpression.Parameters[0], 
                onChange: updatedExpression => { predicateExpression = updatedExpression; });

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
            var originalPredicateExpression = GetLambdaBodyExpression(lambdaExpression);
            BinaryExpression updatedPredicateExpression = null;

            var component = CreateComponent(
                originalPredicateExpression,
                lambdaExpression.Parameters[0],
                updatedExpression => { updatedPredicateExpression = updatedExpression; });

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
            var originalPredicateExpression = GetLambdaBodyExpression(lambdaExpression);
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
            BinaryExpression predicateExpression = null, 
            ParameterExpression parameterExpression = null,
            Action<BinaryExpression> onChange = null,
            Action onRemove = null)
        {
            onChange ??= (_) => { };
            onRemove ??= () => { };

            return RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, predicateExpression ?? _predicateExpression)
                    .Add(p => p.Parameter, parameterExpression ?? _predicateParameter)
                    .Add(p => p.OnChange, onChange)
                    .Add(p => p.OnRemove, onRemove);
            });
        }

        private LambdaExpression GetLambdaExpression<T>(Expression<Func<T, bool>> lambdaExpression) => lambdaExpression;

        private BinaryExpression GetLambdaBodyExpression(LambdaExpression lambdaExpression) => lambdaExpression.Body as BinaryExpression;
        
        public static TheoryData<List<string>> LeftOperandTestData() => 
        [
            // address.PersonId
            [nameof(Address.PersonId)],
            // address.Person.LastName
            [nameof(Address.Person), nameof(Person.LastName)],
            // address.Utilities.Count
            [nameof(Address.Utilities), nameof(List<Utility>.Count)]
        ];

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
            person => person.IsAlive != true
        ];
    }
}
