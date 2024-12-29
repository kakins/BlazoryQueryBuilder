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

            _lambdaExpression = GetLambdaExpression(person => person.PersonId == "1");
            _predicateParameter = _lambdaExpression.Parameters[0];
            _predicateExpression = GetLambdaBodyExpression(_lambdaExpression);
        }

        [Fact]
        public async Task Initializes_field_select_items()
        {
            // Arrange
            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, _predicateExpression)
                    .Add(p => p.Parameter, _predicateParameter)
                    .Add(p => p.OnChange, _ => { });
            });

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

        [Fact]
        public async Task Updates_left_operand_expression_when_selected_field_changes()
        {
            // Arrange
            var predicateExpression = _predicateExpression;
            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, _predicateExpression)
                    .Add(p => p.Parameter, _predicateParameter)
                    .Add(p => p.OnChange, expression => { predicateExpression = expression; });
            });

            // Act
            var fieldSelect = component.FindInputByLabel<MudSelect<string>, string>("Field");
            await component.InvokeAsync(async () =>
            {
                await fieldSelect.Instance.OpenMenu();
                await fieldSelect.Instance.SelectOption(nameof(Person.LastName));
            });

            // Assert
            var leftOperand = predicateExpression.Left as MemberExpression;
            leftOperand.Member.Name.Should().Be(nameof(Person.LastName));
        }

        [Fact]
        public async Task Initializes_operator_select_options()
        {
            // Arrange
            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, _predicateExpression)
                    .Add(p => p.Parameter, _predicateParameter)
                    .Add(p => p.OnChange, _ => { });
            });

            // Act
            var operators = component.FindComponent<RelationalOperators>();

            // Assert
            operators.Instance.ExpressionType.Should().Be(_predicateExpression.NodeType);
        }

        [Fact]
        public async Task Updates_predicate_expression_when_selected_operator_changes()
        {
            // Arrange
            var predicateExpression = _predicateExpression;
            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, _predicateExpression)
                    .Add(p => p.Parameter, _predicateParameter)
                    .Add(p => p.OnChange, updatedExpression => { predicateExpression = updatedExpression; });
            });

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
        public async Task Initializes_int_value_text_field()
        {
            // Arrange
            var lambdaExpression = GetLambdaExpression(person => person.NumberOfChildren == 4);
            var lambdaBodyExpression = GetLambdaBodyExpression(lambdaExpression);

            // Act
            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, lambdaBodyExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Assert
            var valueInput = component.FindInputByLabel<MudTextField<int>, int>("Value");
            valueInput.Instance.Value.Should().Be(4);
        }

        [Fact]
        public async Task Updates_right_operand_expression_when_int_value_changes()
        {
            // Arrange
            var lambdaExpression = GetLambdaExpression(person => person.NumberOfChildren == 4);
            var predicateExpression = GetLambdaBodyExpression(lambdaExpression);

            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, predicateExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, updatedExpression => { predicateExpression = updatedExpression; });
            });

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
        public async Task Initializes_string_value_text_field()
        {
            // Arrange
            var lambdaExpression = GetLambdaExpression(person => person.PersonId == "1");
            var predicateExpression = GetLambdaBodyExpression(lambdaExpression);
            
            // Act
            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, predicateExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Assert
            var valueInput = component.FindInputByLabel<MudTextField<string>, string>("Value");
            valueInput.Instance.Value.Should().Be("1");
        }

        [Fact]
        public async Task Updates_right_operand_expression_when_string_value_changes()
        {
            // Arrange
            var lambdaExpression = GetLambdaExpression(person => person.PersonId == "1");
            var predicateExpression = GetLambdaBodyExpression(lambdaExpression);

            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, predicateExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, updatedExpression => { predicateExpression = updatedExpression; });
            });

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
        public async Task Initializes_date_value_date_picker()
        {
            // Arrange
            var lambdaExpression = GetLambdaExpression(person => person.Created == DateTime.MinValue);
            var predicateExpression = GetLambdaBodyExpression(lambdaExpression).ReplaceRight(DateTimeExpression.New(DateTime.Now));

            // Act
            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, predicateExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Assert
            var valueInput = component
                .FindComponents<MudDatePicker>()
                .FirstOrDefault(p => p.Instance.Label == "Value"); ;
            valueInput.Instance.Date.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public async Task Updates_right_operand_expression_when_date_value_changes()
        {
            // Arrange
            var lambdaExpression = GetLambdaExpression(person => person.Created == DateTime.MinValue);
            var predicateExpression = GetLambdaBodyExpression(lambdaExpression).ReplaceRight(DateTimeExpression.New(DateTime.Now));
            
            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, predicateExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, updatedExpression => { predicateExpression = updatedExpression; });
            });
            
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
        public async Task Initializes_bool_value_check_box()
        {
            // Arrange
            var lambdaExpression = GetLambdaExpression(person => person.IsAlive == true);
            var predicateExpression = GetLambdaBodyExpression(lambdaExpression);

            // Act
            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, predicateExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Assert
            var valueInput = component.FindComponents<MudCheckBox<bool>>().FirstOrDefault(c => c.Instance.Label == "Value"); ;
            valueInput.Instance.Value.Should().BeTrue();
        }

        [Fact]
        public async Task Updates_right_operand_expression_when_bool_value_changes()
        {
            // Arrange
            var lambdaExpression = GetLambdaExpression(person => person.IsAlive == true);
            var predicateExpression = GetLambdaBodyExpression(lambdaExpression);

            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, predicateExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, updatedExpression => { predicateExpression = updatedExpression; });
            });
        
            // Act
            var valueInput = component.FindComponents<MudCheckBox<bool>>().FirstOrDefault(c => c.Instance.Label == "Value");
            await component.InvokeAsync(async () =>
            {
                await valueInput.Instance.ValueChanged.InvokeAsync(false);
            });
            
            // Assert
            var rightOperand = predicateExpression.Right as ConstantExpression;
            rightOperand.Value.Should().Be(false);
        }

        [Fact]
        public async Task Adds_new_expression()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public async Task Removes_expression()
        {
            throw new NotImplementedException();
        }

        private LambdaExpression GetLambdaExpression(Expression<Func<Person, bool>> lambdaExpression)
        {
            return lambdaExpression;
        }

        private BinaryExpression GetLambdaBodyExpression(LambdaExpression lambdaExpression)
        {
            return lambdaExpression.Body as BinaryExpression;
        }

        public static TheoryData<Expression<Func<Person, bool>>> ValueTestExpressions =>
        [
            person => person.PersonId == "1",
            person => person.NumberOfChildren == 4,
            person => person.Created == DateTime.Now,
            person => person.IsAlive == true,
            //person => person.Addresses.Any(address => address.City == "New York")
        ];
    }
}
