using BlazorQueryBuilder.Pages;
using BlazorQueryBuilder.Tests.Util;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using Bunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;
using static BlazorQueryBuilder.Pages.RelationalOperators;

namespace BlazorQueryBuilder.Tests.Pages
{
    public class RelationalOperatorsTests : TestContext
    {
        public RelationalOperatorsTests()
        {
            Services.AddSingleton<PredicateFactory>();

            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;
            JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);

            RenderComponent<MudPopoverProvider>();
        }

        [Theory]
        [MemberData(nameof(OperandTypeData))]
        public async Task Initializes_operator_options_for_operand_type(Type type, LambdaExpression lambdaExpression)
        {
            // Arrange
            var operators = RelationalOperators.GetOperators(type);
            var component = RenderComponent<RelationalOperators>(parameters =>
            {
                parameters.Add(p => p.PredicateExpression, lambdaExpression.Body);
            });

            // Act
            var select = component.FindComponent<MudSelect<ExpressionOperator>>();
            var selectItems = Enumerable.Empty<ExpressionOperator>();
            await component.InvokeAsync(async () =>
            {
                await select.Instance.OpenMenu();
                selectItems = select.Instance.Items.Select(i => i.Value);
            });

            // Assert
            selectItems.Should().BeEquivalentTo(operators);
        }

        [Theory]
        [MemberData(nameof(OperatorData))]
        public async Task Initializes_selected_operator(LambdaExpression lambdaExpression, ExpressionOperator expectedOperator)
        {
            // Arrange
            var component = RenderComponent<RelationalOperators>(parameters =>
            {
                parameters.Add(p => p.PredicateExpression, lambdaExpression.Body);
            });

            // Act
            var select = component.FindComponent<MudSelect<ExpressionOperator>>();
            var selectedOperator = select.Instance.Value;

            // Assert
            select.Instance.Value.Should().BeEquivalentTo(expectedOperator);
            select.Instance.Text.Should().Be(expectedOperator.DisplayText);
        }

        [Fact]
        public async Task Updates_selected_operator()
        {
            // Arrange
            var lambdaExpression = (Expression<Func<Person, bool>>)(p => p.FirstName == "John");
            var component = RenderComponent<RelationalOperators>(parameters =>
            {
                parameters.Add(p => p.PredicateExpression, lambdaExpression.Body);
            });

            // Act
            var select = component.FindComponent<MudSelect<ExpressionOperator>>();
            MudSelectItem<ExpressionOperator> selectedOperator = null;
            await component.InvokeAsync(async () =>
            {
                await select.Instance.OpenMenu();
                var items = select.Instance.Items;
                selectedOperator = items.Single(i => i.Value is NotEqualsOperator);
                await select.Instance.SelectOption(selectedOperator.Value);
            });

            // Assert
            selectedOperator.Value.Should().BeOfType<NotEqualsOperator>();
            select.Instance.Text.Should().Be(selectedOperator.Value.DisplayText);
        }

        [Fact]
        public async Task Updates_operator_options_for_operand_type()
        {
            // Arrange
            var lambdaExpression = (Expression<Func<Person, bool>>)(p => p.FirstName == "John");
            var component = base.RenderComponent<RelationalOperators>(parameters =>
            {
                parameters.Add(p => p.PredicateExpression, lambdaExpression.Body);
            });

            // Act
            var newLambdaExpression = (Expression<Func<Person, bool>>)(p => p.NumberOfChildren == 2);
            component.Instance.UpdateExpression(newLambdaExpression.Body);
            var select = component.FindComponent<MudSelect<ExpressionOperator>>();
            var selectItems = Enumerable.Empty<ExpressionOperator>();
            await component.InvokeAsync(async () =>
            {
                await select.Instance.OpenMenu();
                selectItems = select.Instance.Items.Select(i => i.Value);
            });

            // Assert
            var operators = RelationalOperators.GetOperators(typeof(int));
            selectItems.Should().BeEquivalentTo(operators);
            component.Instance.PredicateExpression.Should().BeEquivalentTo(newLambdaExpression.Body);
        }

        public static TheoryData<Type, Expression<Func<Person, bool>>> OperandTypeData =>
            new()
            {
                { typeof(string), p => p.FirstName == "John" },
                { typeof(int), p => p.NumberOfChildren == 2 },
                { typeof(DateTime), p => p.Created == DateTime.UtcNow },
                { typeof(bool), p => p.IsAlive == true  }
            };

        public static TheoryData<Expression<Func<Person, bool>>, ExpressionOperator> OperatorData()
        {
            return new() 
            {
                { p => p.FirstName == "John", new EqualsOperator() },
                { p => p.FirstName != "John", new NotEqualsOperator() },
                { p => EF.Functions.Like(p.FirstName, "%Alice%"), new EfLikeOperator() },
                { p => !EF.Functions.Like(p.FirstName, "%Alice%"), new EfLikeOperator(true) },
                { p => p.NumberOfChildren == 2, new EqualsOperator() },
                { p => p.NumberOfChildren != 2, new NotEqualsOperator() },
                { p => p.NumberOfChildren < 2, new LessThanOperator() },
                { p => p.NumberOfChildren > 2, new GreaterThanOperator() },
                { p => p.NumberOfChildren >= 2, new GreaterThanOrEqualOperator() },
                { p => p.NumberOfChildren <= 2, new LessThanOrEqualOperator() },
                { p => p.Created == DateTime.UtcNow, new EqualsOperator() },
                { p => p.Created != DateTime.UtcNow, new NotEqualsOperator() },
                { p => p.Created > DateTime.UtcNow, new GreaterThanOperator() },
                { p => p.Created < DateTime.UtcNow, new LessThanOperator() },
                { p => p.Created >= DateTime.UtcNow, new GreaterThanOrEqualOperator() },
                { p => p.Created <= DateTime.UtcNow, new LessThanOrEqualOperator() },
                { p => p.IsAlive == true, new EqualsOperator() },
                { p => p.IsAlive != true, new NotEqualsOperator() },
            };
        }

        [Fact]
        public async Task TestMethodCall()
        {
            Expression<Func<Person, bool>> expression = p => EF.Functions.Like(p.FirstName, "%Alice%");

            var patternValue = "Al%";

            var parameter = Expression.Parameter(typeof(Person), "p");
            var property = Expression.Property(parameter, "FirstName");
            var method = typeof(DbFunctionsExtensions).GetMethod("Like", [typeof(DbFunctions), typeof(string), typeof(string)]);
            var pattern = Expression.Constant(patternValue);
            var methodCall = Expression.Call(null, method, Expression.Constant(EF.Functions), property, pattern);
            var like = Expression.Lambda<Func<Person, bool>>(methodCall, parameter);

            var notMethodCall = Expression.Not(methodCall);
            var notlike = Expression.Lambda<Func<Person, bool>>(notMethodCall, parameter);

            var options = EfHelpers.CreateEfInMemoryContextOptions<MyDbContext>("TestDb");
            var dbContext = new MyDbContext(options);
            await dbContext.SeedDatabase();

            var query1 = dbContext.Persons.Where(like).Select(p => p.FirstName).ToList();
            var query2 = dbContext.Persons.Where(notlike).Select(p => p.FirstName).ToList();

        }
    }
}
