using BlazorQueryBuilder.ExpressionVisitors;
using BlazorQueryBuilder.Pages;
using BlazorQueryBuilder.Tests.Util;
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
    public class RelationalPredicateTests : TestContext
    {
        private Expression<Func<Person, bool>> _lambdaExpression;
        private BinaryExpression _lambdaBodyExpression;

        public RelationalPredicateTests()
        {
            Services.AddSingleton<PredicateFactory>();

            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;
            JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);

            RenderComponent<MudPopoverProvider>();

            _lambdaExpression = person => person.PersonId == "1";
            _lambdaBodyExpression = GetLambdaBodyExpression(_lambdaExpression);
        }

        [Fact]
        public async Task Initializes_field_select_items()
        {
            // Arrange
            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.Binary, _lambdaBodyExpression)
                    .Add(p => p.Parameter, _lambdaExpression.Parameters[0])
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
            var expectedFieldSelectValue = ((MemberExpression)component.Instance.Binary.Left).Member.Name;
            fieldSelect.Instance.Value.Should().Be(expectedFieldSelectValue);

            var expectedFieldSelectItems = _lambdaExpression.Parameters[0].Type.GetProperties().Select(p => p.Name);
            fieldItems
                .Select(i => i.Value)
                .Should()
                .BeEquivalentTo(expectedFieldSelectItems);
        }

        [Fact]
        public async Task Updates_expression_when_selected_field_changes()
        {
            // Arrange
            var binaryExpression = _lambdaBodyExpression;
            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.Binary, _lambdaBodyExpression)
                    .Add(p => p.Parameter, _lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, expression => { binaryExpression = expression; });
            });

            // Act
            var fieldSelect = component.FindInputByLabel<MudSelect<string>, string>("Field");
            await component.InvokeAsync(async () =>
            {
                await fieldSelect.Instance.OpenMenu();
                await fieldSelect.Instance.SelectOption(nameof(Person.LastName));
            });

            // Assert
            var updatedMemberExpression = binaryExpression.Left as MemberExpression;
            updatedMemberExpression.Member.Name.Should().Be(nameof(Person.LastName));
        }

        [Fact]
        public async Task Initializes_operator_select_options()
        {
            // Arrange
            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.Binary, _lambdaBodyExpression)
                    .Add(p => p.Parameter, _lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Act
            var operators = component.FindComponent<RelationalOperators>();

            // Assert
            operators.Instance.ExpressionType.Should().Be(_lambdaBodyExpression.NodeType);
        }

        [Fact]
        public async Task Updates_expression_when_selected_operator_changes()
        {
            // Arrange
            var binaryExpression = _lambdaBodyExpression;
            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.Binary, _lambdaBodyExpression)
                    .Add(p => p.Parameter, _lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, updatedExpression => { binaryExpression = updatedExpression; });
            });

            // Act
            var operators = component.FindComponent<RelationalOperators>();
            await component.InvokeAsync(() =>
            {
                operators.Instance.OnChange(ExpressionType.NotEqual);
            });

            // Assert
            binaryExpression.NodeType.Should().Be(ExpressionType.NotEqual);
        }

        [Fact]
        public async Task Initializes_int_value()
        {
            // Arrange
            Expression<Func<Person, bool>> lambdaExpression = person => person.NumberOfChildren == 4;
            var lambdaBodyExpression = GetLambdaBodyExpression(lambdaExpression);

            // Act
            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.Binary, lambdaBodyExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Assert
            var valueInput = component.FindInputByLabel<MudTextField<int>, int>("Value");
            valueInput.Instance.Value.Should().Be(4);
        }

        [Fact]
        public async Task Updates_expression_when_int_value_changes()
        {
            // Arrange
            Expression<Func<Person, bool>> lambdaExpression = person => person.NumberOfChildren == 4;
            var lambdaBodyExpression = GetLambdaBodyExpression(lambdaExpression);

            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.Binary, lambdaBodyExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, updatedExpression => { lambdaBodyExpression = updatedExpression; });
            });

            // Act
            var valueInput = component.FindInputByLabel<MudTextField<int>, int>("Value");
            await component.InvokeAsync(() =>
            {
                valueInput.Instance.SetText("5");
            });

            // Assert
            var updatedConstantExpression = lambdaBodyExpression.Right as ConstantExpression;
            updatedConstantExpression.Value.Should().Be(5);
        }

        [Fact]
        public async Task Initializes_string_value()
        {
            // Arrange
            Expression<Func<Person, bool>> lambdaExpression = person => person.PersonId == "1";
            var lambdaBodyExpression = GetLambdaBodyExpression(lambdaExpression);
            
            // Act
            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.Binary, lambdaBodyExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Assert
            var valueInput = component.FindInputByLabel<MudTextField<string>, string>("Value");
            valueInput.Instance.Value.Should().Be("1");
        }

        [Fact]
        public async Task Updates_expression_when_string_value_changes()
        {
            // Arrange
            Expression<Func<Person, bool>> lambdaExpression = person => person.PersonId == "1";
            var lambdaBodyExpression = GetLambdaBodyExpression(lambdaExpression);
            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.Binary, lambdaBodyExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, updatedExpression => { lambdaBodyExpression = updatedExpression; });
            });

            // Act
            var valueInput = component.FindInputByLabel<MudTextField<string>, string>("Value");
            await component.InvokeAsync(() =>
            {
                valueInput.Instance.SetText("2");
            });
            
            // Assert
            var updatedConstantExpression = lambdaBodyExpression.Right as ConstantExpression;
            updatedConstantExpression.Value.Should().Be("2");
        }

        [Fact]
        public async Task Initializes_date_value()
        {
            // Arrange
            var ticksExpression = Expression.Constant(DateTime.Now.Ticks, typeof(long));
            var newDateTimeExpression = Expression.New(typeof(DateTime).GetConstructor([typeof(long)]), ticksExpression);

            Expression<Func<Person, bool>> lambdaExpression = person => person.Created == DateTime.MinValue;
            var lambdaBodyExpression = GetLambdaBodyExpression(lambdaExpression);
            lambdaBodyExpression = lambdaBodyExpression.ReplaceRight(newDateTimeExpression);

            // Act
            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.Binary, lambdaBodyExpression)
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
        public async Task Updates_expression_when_date_value_changes()
        {
            // Arrange
            var ticksExpression = Expression.Constant(DateTime.Now.Ticks, typeof(long));
            var newDateTimeExpression = Expression.New(typeof(DateTime).GetConstructor([typeof(long)]), ticksExpression);
            Expression<Func<Person, bool>> lambdaExpression = person => person.Created == DateTime.MinValue;
            var lambdaBodyExpression = GetLambdaBodyExpression(lambdaExpression);
            lambdaBodyExpression = lambdaBodyExpression.ReplaceRight(newDateTimeExpression);
            
            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.Binary, lambdaBodyExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, updatedExpression => { lambdaBodyExpression = updatedExpression; });
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
            var updatedConstantExpression = lambdaBodyExpression.Right as NewExpression;
            var value = updatedConstantExpression.Arguments[0] as ConstantExpression;
            value.Value.Should().Be(dateTime.Ticks);
        }

        [Fact]
        public async Task Initializes_bool_value()
        {
            // Arrange
            Expression<Func<Person, bool>> lambdaExpression = person => person.IsAlive == true;
            var lambdaBodyExpression = GetLambdaBodyExpression(lambdaExpression);

            // Act
            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.Binary, lambdaBodyExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Assert
            var valueInput = component.FindComponents<MudCheckBox<bool>>().FirstOrDefault(c => c.Instance.Label == "Value"); ;
            valueInput.Instance.Value.Should().BeTrue();
        }

        [Fact]
        public async Task Updates_expression_when_bool_value_changes()
        {
            // Arrange
            Expression<Func<Person, bool>> lambdaExpression = person => person.IsAlive == true;
            var lambdaBodyExpression = GetLambdaBodyExpression(lambdaExpression);
            var component = RenderComponent<RelationalPredicate>(parameters =>
            {
                parameters
                    .Add(p => p.Binary, lambdaBodyExpression)
                    .Add(p => p.Parameter, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, updatedExpression => { lambdaBodyExpression = updatedExpression; });
            });
        
            // Act
            var valueInput = component.FindComponents<MudCheckBox<bool>>().FirstOrDefault(c => c.Instance.Label == "Value");
            await component.InvokeAsync(async () =>
            {
                await valueInput.Instance.ValueChanged.InvokeAsync(false);
            });
            
            // Assert
            var updatedConstantExpression = lambdaBodyExpression.Right as ConstantExpression;
            updatedConstantExpression.Value.Should().Be(false);
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

        private BinaryExpression GetLambdaBodyExpression(Expression<Func<Person, bool>> lambdaExpression)
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
