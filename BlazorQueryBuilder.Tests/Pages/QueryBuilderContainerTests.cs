using BlazorQueryBuilder.Pages;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using Bunit;
using FluentAssertions;
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

        public QueryBuilderContainerTests()
        {
            Services.AddSingleton<PredicateFactory>();
            Services.AddSingleton<QueryBuilderService<Person>>();
            Services.AddSingleton<QueryServiceFactory<MyDbContext>>();
            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;
            JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);

            _popoverProvider = RenderComponent<MudPopoverProvider>();
        }

        [Fact]
        public void Loads_component()
        {
            // Arrange & Act
            var component = RenderComponent<QueryBuilderContainer>();

            // Assert
            component.Should().NotBeNull();
        }

        [Fact]
        public void Displays_new_query_button_on_load()
        {
            // Arrange & Act
            var component = RenderComponent<QueryBuilderContainer>();

            var newQueryButton = component
                .FindComponents<MudButton>()
                .Single(button => button.Markup.Contains("New Query"));

            // Assert            
            newQueryButton.Should().NotBeNull();
        }

        [Fact]
        public async Task Displays_query_builder_for_new_query()
        {
            // Arrange
            var component = RenderComponent<QueryBuilderContainer>();
            
            // Act
            var newQueryButton = component
                .FindComponents<MudButton>()
                .Single(button => button.Markup.Contains("New Query"));

            await component.InvokeAsync(newQueryButton.Instance.OnClick.InvokeAsync);

            // Assert
            var queryBuilder = component.FindComponent<QueryBuilder<Person>>();
            queryBuilder.Should().NotBeNull();
            queryBuilder.Instance.Expression.Should().BeEmpty();
        }

        [Fact]
        public async Task Displays_query_builder_for_loaded_query()
        {
            // Arrange
            var component = RenderComponent<QueryBuilderContainer>();

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

            await component.InvokeAsync(loadQueryButton.Instance.OnClick.InvokeAsync);

            // Assert
            var queryBuilder = component.FindComponent<QueryBuilder<Person>>();
            queryBuilder.Should().NotBeNull();
            queryBuilder.Instance.Expression.Should().Be(selectedQuery);
        }
    }
}
