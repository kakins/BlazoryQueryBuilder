﻿using BlazorQueryBuilder.Pages;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using Bunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace BlazorQueryBuilder.Tests.Pages
{
    public class QueryBuilderContainerTests : TestContext
    {
        private readonly IRenderedComponent<MudPopoverProvider> _popoverProvider;
        private readonly MyDbContext _dbContext;

        public QueryBuilderContainerTests()
        {
            Services.AddSingleton<PredicateFactory>();
            Services.AddSingleton<QueryBuilderService<Person>>();
            Services.AddSingleton<QueryBuilderService<Address>>();
            Services.AddSingleton<IQueryServiceFactory<MyDbContext>, QueryServiceFactory<MyDbContext>>();
            Services.AddMudServices();

            var dbContextOptionsBuilder = new DbContextOptionsBuilder<MyDbContext>().UseInMemoryDatabase("test");
            _dbContext = new MyDbContext(dbContextOptionsBuilder.Options);
            Services.AddSingleton(provider => _dbContext);

            JSInterop.Mode = JSRuntimeMode.Loose;
            JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);

            _popoverProvider = RenderComponent<MudPopoverProvider>();
        }

        [Fact]
        public async Task Displays_query_builder_for_new_query()
        {
            // Arrange
            var component = RenderComponent<QueryBuilderContainer<MyDbContext>>();

            // Act
            component
                .FindComponents<MudButton>()
                .Single(button => button.Markup.Contains("New Query"))
                .Find("button")
                .Click();

            // Assert
            var queryBuilder = component.FindComponent<QueryBuilder<MyDbContext, Address>>();
            queryBuilder.Should().NotBeNull();
            queryBuilder.Instance.Expression.Should().BeEmpty();
        }

        [Fact]
        public async Task Loads_enities_for_new_query()
        {
            // Arrange
            var component = RenderComponent<QueryBuilderContainer<MyDbContext>>();
            component
                .FindComponents<MudButton>()
                .Single(button => button.Markup.Contains("New Query"))
                .Find("button")
                .Click();
            
            // Act
            var entitiesSelect = component.FindComponents<MudSelect<string>>().FirstOrDefault();
            var entityItems = entitiesSelect.Instance.Items.ToList();
            await component.InvokeAsync(async () =>
            {
                await entitiesSelect.Instance.OpenMenu();
                entityItems = [.. entitiesSelect.Instance.Items];
                await entitiesSelect.Instance.SelectOption(0);
            });

            // Assert
            entityItems
                .Select(i => i.Value)
                .Should()
                .BeEquivalentTo(_dbContext.Model.GetEntityTypes().Select(e => e.ClrType.Name));
        }

        [Fact]
        public async Task Updates_query_builder_for_selected_entity()
        {
            // Arrange
            var component = RenderComponent<QueryBuilderContainer<MyDbContext>>();
            
            component
                .FindComponents<MudButton>()
                .Single(button => button.Markup.Contains("New Query"))
                .Find("button")
                .Click();

            var entitiesSelect = component.FindComponent<MudSelect<string>>();
            
            await component.InvokeAsync(async () =>
            {
                await entitiesSelect.Instance.OpenMenu();
                var addressSelection = entitiesSelect.Instance.Items.First(i => i.Value == nameof(Address)).Value;
                await entitiesSelect.Instance.SelectOption(addressSelection);
            });

            // Act
            await component.InvokeAsync(async () =>
            {
                await entitiesSelect.Instance.OpenMenu();
                var personSelection = entitiesSelect.Instance.Items.First(i => i.Value == nameof(Person)).Value;
                await entitiesSelect.Instance.SelectOption(personSelection);
            });

            // Assert
            component.HasComponent<QueryBuilder<MyDbContext, Address>>().Should().BeFalse();
            component.HasComponent<QueryBuilder<MyDbContext, Person>>().Should().BeTrue();
        }

        [Fact]
        public async Task Disables_loading_queries_when_no_query_is_specified()
        {
            // Arrange
            var component = RenderComponent<QueryBuilderContainer<MyDbContext>>();

            // Act
            var loadQueryButton = component
                .FindComponents<MudButton>()
                .Single(button => button.Markup.Contains("Load Query"));

            // Assert
            loadQueryButton.Instance.Disabled.Should().BeTrue();
        }

        [Fact]
        public async Task Enables_loading_queries_when_query_is_specified()
        {
            // Arrange
            var component = RenderComponent<QueryBuilderContainer<MyDbContext>>();

            var select = component.FindComponent<MudSelect<string>>();
            string selectedQuery = string.Empty;
            await component.InvokeAsync(async () =>
            {
                await select.Instance.OpenMenu();
                await select.Instance.SelectOption(0);
                await select.Instance.ToggleMenu();
                selectedQuery = select.Instance.Items.First().Value;
            });

            // Act
            var loadQueryButton = component
                .FindComponents<MudButton>()
                .Single(button => button.Markup.Contains("Load Query"));

            // Assert
            loadQueryButton.Instance.Disabled.Should().BeFalse();
        }

        [Fact]
        public async Task Displays_query_builder_for_loaded_query()
        {
            // Arrange
            var component = RenderComponent<QueryBuilderContainer<MyDbContext>>();

            var select = component.FindComponent<MudSelect<string>>();
            string selectedQuery = string.Empty;
            await component.InvokeAsync(async () =>
            {
                await select.Instance.OpenMenu();
                await select.Instance.SelectOption(0);
                await select.Instance.ToggleMenu();
                selectedQuery = select.Instance.Items.First().Value;
            });

            // Act
            component
                .FindComponents<MudButton>()
                .Single(button => button.Markup.Contains("Load Query"))
                .Find("button")
                .Click();

            // Assert
            var queryBuilder = component.FindComponent<QueryBuilder<MyDbContext, Person>>();
            queryBuilder.Should().NotBeNull();
            queryBuilder.Instance.Expression.Should().Be(selectedQuery);
        }
    }
}
