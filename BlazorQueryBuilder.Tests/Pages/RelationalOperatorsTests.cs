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
using System.Collections.Generic;
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
        [MemberData(nameof(OperatorsData))]
        public async Task Displays_operators_for_type(Type type, List<Operator> operators)
        {
            // Arrange
            var component = RenderComponent<RelationalOperators>(parameters =>
            {
                parameters.Add(p => p.OperandType, type);
                parameters.Add(p => p.ExpressionOperator, operators.First());
            });

            // Act
            var select = component.FindComponent<MudSelect<Operator>>();
            var selectItems = Enumerable.Empty<Operator>();
            await component.InvokeAsync(async () =>
            {
                await select.Instance.OpenMenu();
                selectItems = select.Instance.Items.Select(i => i.Value);
            });

            // Assert
            selectItems.Should().BeEquivalentTo(operators);
        }

        [Fact]
        public async Task Initializes_standard_selected_operator()
        {
            // Arrange
            var op = new Operator { ExpressionType = ExpressionType.Equal };
            var component = RenderComponent<RelationalOperators>(parameters =>
            {
                parameters.Add(p => p.OperandType, typeof(string));
                parameters.Add(p => p.ExpressionOperator, op);
            });

            // Act
            var select = component.FindComponent<MudSelect<Operator>>();
            var selectedOperator = select.Instance.Value;

            // Assert
            select.Instance.Value.Should().BeEquivalentTo(op);
            // TODO: get the text value from a lookup/mapping
            select.Instance.Text.Should().Be("Equals");
        }

        [Fact]
        public async Task Initializes_method_selected_operator()
        {
            // Arrange
            var op = new EfLikeOperator();
            var component = RenderComponent<RelationalOperators>(parameters =>
            {
                parameters.Add(p => p.OperandType, typeof(string));
                parameters.Add(p => p.ExpressionOperator, op);
            });

            // Act
            var select = component.FindComponent<MudSelect<Operator>>();
            var selectedOperator = select.Instance.Value;

            // Assert
            select.Instance.Value.Should().BeEquivalentTo(op);
            select.Instance.Text.Should().Be(op.DisplayText);
        }

        [Fact]
        public async Task Updated_selected_operator()
        {
            // Arrange
            Operator op = new Operator { ExpressionType = ExpressionType.Equal };
            var component = RenderComponent<RelationalOperators>(parameters =>
            {
                parameters.Add(p => p.OperandType, typeof(string));
                parameters.Add(p => p.ExpressionOperator, op);
                parameters.Add(p => p.OnChange, (Operator updatedOperator) => op = updatedOperator);
            });

            // Act
            var select = component.FindComponent<MudSelect<Operator>>();
            Operator selectedOperator = null;
            await component.InvokeAsync(async () =>
            {
                await select.Instance.OpenMenu();
                var items = select.Instance.Items;
                selectedOperator = items.First(i => i.Value != op).Value;
                await select.Instance.SelectOption(selectedOperator);
            });

            // Assert
            op.Should().Be(selectedOperator);
            select.Instance.Text.Should().Be(op.DisplayText);
        }

        public static TheoryData<Type, List<Operator>> OperatorsData =>
            new()
            {
                {
                    typeof(string),
                    new()
                    {
                        new Operator { ExpressionType = ExpressionType.Equal },
                        new Operator { ExpressionType = ExpressionType.NotEqual },
                        new EfLikeOperator(),
                        new EfLikeOperator(true)
                    }
                },
                {
                    typeof(int),
                    new()
                    {
                        new Operator { ExpressionType = ExpressionType.Equal },
                        new Operator { ExpressionType = ExpressionType.NotEqual },
                        new Operator { ExpressionType = ExpressionType.GreaterThan },
                        new Operator { ExpressionType = ExpressionType.GreaterThanOrEqual },
                        new Operator { ExpressionType = ExpressionType.LessThan },
                        new Operator { ExpressionType = ExpressionType.LessThanOrEqual }
                    }
                },
                {
                    typeof(DateTime),
                    new()
                    {
                        new Operator { ExpressionType = ExpressionType.Equal },
                        new Operator { ExpressionType = ExpressionType.NotEqual },
                        new Operator { ExpressionType = ExpressionType.GreaterThan },
                        new Operator { ExpressionType = ExpressionType.GreaterThanOrEqual },
                        new Operator { ExpressionType = ExpressionType.LessThan },
                        new Operator { ExpressionType = ExpressionType.LessThanOrEqual }
                    }
                },
                {
                    typeof(bool),
                    new()
                    {
                        new Operator { ExpressionType = ExpressionType.Equal },
                        new Operator { ExpressionType = ExpressionType.NotEqual }
                    }
                }
            };

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
