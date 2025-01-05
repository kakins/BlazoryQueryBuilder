using BlazorQueryBuilder.Pages;
using BlazorQueryBuilder.Tests.Util;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using BlazoryQueryBuilder.Shared.Util;
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
        public async Task Initializes_options(Type type, LambdaExpression lambdaExpression)
        {
            // Arrange
            var operators = RelationalOperators.GetOperators(type);
            var component = RenderComponent<RelationalOperators>(parameters =>
            {
                parameters.Add(p => p.PredicateExpression, lambdaExpression.Body);
            });

            // Act
            var select = component.FindComponent<MudSelect<ExpressionOperator>>();
            var selectItems = await component.InvokeAsync(async () =>
            {
                await select.Instance.OpenMenu();
                return select.Instance.Items.Select(i => i.Value);
            });

            // Assert
            selectItems.Should().BeEquivalentTo(operators);
        }

        [Theory]
        [MemberData(nameof(OperatorData))]
        public async Task Initializes_selected_operator(Type type, LambdaExpression lambdaExpression, ExpressionOperator expectedOperator)
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
        public async Task Updates_selected_binary_operator()
        {
            // Arrange
            var lambdaExpression = (Expression<Func<Person, bool>>)(p => p.FirstName == "John");
            var onChange = new Mock<Action<Expression>>();
            var component = RenderComponent<RelationalOperators>(parameters =>
            {
                parameters.Add(p => p.PredicateExpression, lambdaExpression.Body);
                parameters.Add(p => p.OnChange, onChange.Object);
            });

            // Act
            var select = component.FindComponent<MudSelect<ExpressionOperator>>();
            var selectedOperator = await component.InvokeAsync(async () =>
            {
                await select.Instance.OpenMenu();
                var notEquals = select.Instance.Items.Single(i => i.Value is NotEqualsOperator);
                await select.Instance.SelectOption(notEquals.Value);
                return notEquals;
            });

            // Assert
            selectedOperator.Value.Should().BeOfType<NotEqualsOperator>();
            select.Instance.Text.Should().Be(selectedOperator.Value.DisplayText);
            onChange.Verify(o => o(It.IsAny<BinaryExpression>()), Times.Once);
        }

        [Fact]
        public async Task Preserves_binary_constant_value_for_updated_binary_operator()
        {
            // Arrange
            var lambdaExpression = (Expression<Func<Person, bool>>)(p => p.FirstName == "John");
            BinaryExpression updatedExpression = null;
            var component = RenderComponent<RelationalOperators>(parameters =>
            {
                parameters.Add(p => p.PredicateExpression, lambdaExpression.Body);
                parameters.Add(p => p.OnChange, (exp) => updatedExpression = (BinaryExpression)exp);
            });

            // Act
            var select = component.FindComponent<MudSelect<ExpressionOperator>>();
            var selectedOperator = await component.InvokeAsync(async () =>
            {
                await select.Instance.OpenMenu();
                var notEquals = select.Instance.Items.Single(i => i.Value is NotEqualsOperator);
                await select.Instance.SelectOption(notEquals.Value);
                return notEquals;
            });

            // Assert
            var constantValue = (ConstantExpression)updatedExpression.Right;
            constantValue.Value.Should().Be("John");
        }

        [Theory]
        [MemberData(nameof(UpdateMethodCallOperatorData))]
        public async Task Updates_selected_method_call_operator(LambdaExpression lambdaExpression, ExpressionType updatedExpressionType, MethodCallOperator op)
        {
            var onChange = new Mock<Action<Expression>>();
            var component = RenderComponent<RelationalOperators>(parameters =>
            {
                parameters.Add(p => p.PredicateExpression, lambdaExpression.Body);
                parameters.Add(p => p.OnChange, onChange.Object);
            });

            // Act
            var select = component.FindComponent<MudSelect<ExpressionOperator>>();
            var selectedOperator = await component.InvokeAsync(async () =>
            {
                await select.Instance.OpenMenu();
                // Select the negated inverse of the current operator
                var selectItem = select
                    .Instance
                    .Items
                    .Select(i => i.Value)
                    .OfType<MethodCallOperator>()
                    .Single(i => i.GetType() == op.GetType() && op.IsNegated != i.IsNegated);
                await select.Instance.SelectOption(selectItem);
                return selectItem;
            });

            // Assert
            selectedOperator.Should().BeAssignableTo(op.GetType());
            select.Instance.Text.Should().Be(selectedOperator.DisplayText);
            // The updated expression should be the selected negated inverse 
            if (op.IsNegated)
            {
                onChange.Verify(o => o(It.IsAny<MethodCallExpression>()), Times.Once);
            }
            else
            {
                onChange.Verify(o => o(It.Is<UnaryExpression>(e => e.NodeType == ExpressionType.Not)), Times.Once);
            }
        }

        [Theory]
        [MemberData(nameof(OperatorData))]
        public async Task Updates_operator_options_for_updated_expression(Type type, LambdaExpression newLambdaExpression, ExpressionOperator op)
        {
            // Arrange
            var lambdaExpression = (Expression<Func<Person, bool>>)(p => p.FirstName == "John");
            var component = base.RenderComponent<RelationalOperators>(parameters =>
            {
                parameters.Add(p => p.PredicateExpression, lambdaExpression.Body);
            });

            // Act
            component.Instance.UpdateExpression(newLambdaExpression.Body);
            var select = component.FindComponent<MudSelect<ExpressionOperator>>();
            var selectItems = await component.InvokeAsync(async () =>
            {
                await select.Instance.OpenMenu();
                return select.Instance.Items.Select(i => i.Value);
            });

            // Assert
            selectItems.Should().BeEquivalentTo(RelationalOperators.GetOperators(type));
            component.Instance.PredicateExpression.Should().BeEquivalentTo(newLambdaExpression.Body);
        }

        public static TheoryData<Type, Expression<Func<Person, bool>>> OperandTypeData => new()
        {
            { typeof(string), p => p.FirstName == "John" },
            { typeof(int), p => p.NumberOfChildren == 2 },
            { typeof(DateTime), p => p.Created == DateTime.UtcNow },
            { typeof(bool), p => p.IsAlive == true  }
        };

        public static TheoryData<Expression<Func<Person, bool>>, ExpressionType, MethodCallOperator> UpdateMethodCallOperatorData() => new()
        {
            { p => EF.Functions.Like(p.FirstName, "%Alice%"), ExpressionType.Not, new EfLikeOperator() },
            { p => !EF.Functions.Like(p.FirstName, "%Alice%"), ExpressionType.Call, new EfLikeOperator(true) },
            { p => new[] {"Alice", "Bob" }.Contains(p.FirstName), ExpressionType.Not, new InListOperator<string>() },
            { p => !new[] {"Alice", "Bob" }.Contains(p.FirstName), ExpressionType.Call, new InListOperator<string>(true) }
        };

        public static TheoryData<Type, Expression<Func<Person, bool>>, ExpressionOperator> OperatorData() => new()
        {
            { typeof(string), p => p.FirstName == "John", new EqualsOperator() },
            { typeof(string), p => p.FirstName != "John", new NotEqualsOperator() },
            { typeof(string), p => EF.Functions.Like(p.FirstName, "%Alice%"), new EfLikeOperator() },
            { typeof(string), p => !EF.Functions.Like(p.FirstName, "%Alice%"), new EfLikeOperator(true) },
            { typeof(int), p => p.NumberOfChildren == 2, new EqualsOperator() },
            { typeof(int), p => p.NumberOfChildren != 2, new NotEqualsOperator() },
            { typeof(int), p => p.NumberOfChildren < 2, new LessThanOperator() },
            { typeof(int), p => p.NumberOfChildren > 2, new GreaterThanOperator() },
            { typeof(int), p => p.NumberOfChildren >= 2, new GreaterThanOrEqualOperator() },
            { typeof(int), p => p.NumberOfChildren <= 2, new LessThanOrEqualOperator() },
            { typeof(DateTime), p => p.Created == DateTime.UtcNow, new EqualsOperator() },
            { typeof(DateTime), p => p.Created != DateTime.UtcNow, new NotEqualsOperator() },
            { typeof(DateTime), p => p.Created > DateTime.UtcNow, new GreaterThanOperator() },
            { typeof(DateTime), p => p.Created < DateTime.UtcNow, new LessThanOperator() },
            { typeof(DateTime), p => p.Created >= DateTime.UtcNow, new GreaterThanOrEqualOperator() },
            { typeof(DateTime), p => p.Created <= DateTime.UtcNow, new LessThanOrEqualOperator() },
            { typeof(bool), p => p.IsAlive == true, new EqualsOperator() },
            { typeof(bool), p => p.IsAlive != true, new NotEqualsOperator() },
            { typeof(string), p => new[] {"Alice, Bob" }.Contains(p.FirstName), new InListOperator<string>() },
            { typeof(string), p => !new[] {"Alice, Bob" }.Contains(p.FirstName), new InListOperator<string>(true) },
            { typeof(int), p => new[] {1, 2 }.Contains(p.NumberOfChildren), new InListOperator<int>() },
            { typeof(int), p => !new[] {1, 2 }.Contains(p.NumberOfChildren), new InListOperator<int>(true) }
        };

        [Fact]
        public async Task MethodCallLike()
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

        [Fact]
        public async Task MethodCallContains()
        {
            Expression<Func<Person, bool>> inListLambda = p => new[] { "Alice", "Bob" }.Contains(p.FirstName);
            Expression<Func<Person, bool>> notInListLambda = p => !new[] { "Alice", "Bob" }.Contains(p.FirstName);

            var values = new[] { "Alice", "Bob" };
            var parameter = Expression.Parameter(typeof(Person), "p");
            var property = Expression.Property(parameter, "FirstName");
            var method = EnumerableMethodInfo.Contains<string>();
            var list = Expression.Constant(values);

            var methodCall = Expression.Call(null, method, list, property);

            var inList = Expression.Lambda<Func<Person, bool>>(methodCall, parameter);

            var notMethodCall = Expression.Not(methodCall);
            var notInList = Expression.Lambda<Func<Person, bool>>(notMethodCall, parameter);

            var options = EfHelpers.CreateEfInMemoryContextOptions<MyDbContext>("TestDb");
            var dbContext = new MyDbContext(options);
            await dbContext.SeedDatabase();

            inListLambda.Should().BeEquivalentTo(inList);
            notInListLambda.Should().BeEquivalentTo(notInList);

            var query1 = dbContext.Persons.Where(inList).Select(p => p.FirstName).ToList();
            var query2 = dbContext.Persons.Where(notInList).Select(p => p.FirstName).ToList();
        }
    }
}
