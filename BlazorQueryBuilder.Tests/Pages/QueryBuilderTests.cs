using BlazorQueryBuilder.Pages;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace BlazorQueryBuilder.Tests.Pages
{
    public class QueryBuilderTests : TestContext
    {
        private readonly IRenderedComponent<MudPopoverProvider> _popoverProvider;
        private readonly List<Person> _persons;
        private readonly List<Address> _addresses;

        public QueryBuilderTests()
        {
            _persons =
            [
                new Person { PersonId = "1", FirstName = "Casey", LastName = "Jones" },
                new Person { PersonId = "2", FirstName = "Master", LastName = "Splinter" },
            ];
            _addresses =
            [
                new Address { PersonId = "1", AddressId = 1 }
            ];

            Services.AddSingleton<PredicateFactory>();
            Services.AddSingleton<QueryBuilderService<Person>>();
            Services.AddSingleton<QueryBuilderService<Address>>();
            Services.AddSingleton(provider => CreateQueryServiceFactory().Object);

            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;
            JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);

            _popoverProvider = RenderComponent<MudPopoverProvider>();
        }

        [Fact]
        public async Task Loads_selected_fields_dropdown()
        {
            // Arrange
            var component = RenderComponent<QueryBuilder<MyDbContext, Person>>();

            // Act
            var fieldsSelect = component
                .FindComponents<MudSelect<PropertyInfo>>()
                .Where(c => c.Instance.Label == "Fields")
                .FirstOrDefault();
            var fieldItems = fieldsSelect.Instance.Items.ToList();
            await component.InvokeAsync(async () =>
            {
                await fieldsSelect.Instance.OpenMenu();
                fieldItems = [.. fieldsSelect.Instance.Items];
            });

            // Assert
            fieldItems
                .Select(i => i.Value)
                .Should()
                .BeEquivalentTo(typeof(Person).GetProperties());
        }

        [Fact]
        public async Task Initializes_loaded_query_expression()
        {
            // Arrange
            var expression = "person => (person.PersonId == \"1\")";
            var component = RenderComponent<QueryBuilder<MyDbContext, Person>>(parameters =>
            {
                parameters.Add(p => p.Expression, expression);
            });

            // Act
            var lambdaComponent = component.FindComponent<LambdaComponent>();

            // Assert
            lambdaComponent.Instance.Lambda.ToString().Should().Be(expression);
        }

        [Fact]
        public async Task Displays_query_results()
        {
            // Arrange
            var component = RenderComponent<QueryBuilder<MyDbContext, Person>>();
            var fieldsSelect = component
                .FindComponents<MudSelect<PropertyInfo>>()
                .Where(c => c.Instance.Label == "Fields")
                .FirstOrDefault();

            await component.InvokeAsync(async () =>
            {
                await fieldsSelect.Instance.OpenMenu();
                foreach (var item in fieldsSelect.Instance.Items)
                {
                    await fieldsSelect.Instance.SelectOption(item.Value);
                }
            });

            // Act
            component
                .FindComponents<MudButton>()
                .Single(button => button.Markup.Contains("Run Query"))
                .Find("button")
                .Click();

            // Assert
            var resultsTable = component.FindComponent<MudTable<Person>>();

            resultsTable.Instance.Items.Should().BeEquivalentTo(_persons);

            var expectedHeaders = fieldsSelect.Instance.Items.Select(item => item.Value.Name).ToList();
            var headers = resultsTable
                .FindAll("th")
                .Select(th => th.TextContent)
                .ToList()
                .Should()
                .BeEquivalentTo(expectedHeaders);
        }

        private Mock<IQueryServiceFactory<MyDbContext>> CreateQueryServiceFactory()
        {
            var queryServiceFactory = new Mock<IQueryServiceFactory<MyDbContext>>();
            queryServiceFactory
                .Setup(factory => factory.Create<Person>())
                .Returns(CreateQueryService(_persons));
            queryServiceFactory
                .Setup(factory => factory.Create<Address>())
                .Returns(CreateQueryService(_addresses));

            IQueryService<TEntity> CreateQueryService<TEntity>(IEnumerable<TEntity> data) where TEntity : class
            {
                var queryService = new Mock<IQueryService<TEntity>>();
                queryService
                    .Setup(service => service.QueryData(It.IsAny<Expression<Func<TEntity, bool>>>(), It.IsAny<IEnumerable<string>>()))
                    .ReturnsAsync(data);
                return queryService.Object;
            }

            return queryServiceFactory;
        }
    }
}
