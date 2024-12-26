using BlazorQueryBuilder.Pages;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using Bunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudBlazor;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;

namespace BlazorQueryBuilder.Tests.Pages
{
    public class QueryBuilderTests : TestContext
    {
        private readonly MyDbContext _dbContext;
        private readonly IRenderedComponent<MudPopoverProvider> _popoverProvider;

        public QueryBuilderTests()
        {
            Services.AddSingleton<PredicateFactory>();
            Services.AddSingleton<QueryBuilderService<Person>>();
            Services.AddSingleton<QueryBuilderService<Address>>();
            Services.AddSingleton<QueryServiceFactory<MyDbContext>>();
            Services.AddMudServices();

            var dbContextOptionsBuilder = new DbContextOptionsBuilder<MyDbContext>().UseInMemoryDatabase("test");
            _dbContext = new MyDbContext(dbContextOptionsBuilder.Options);
            Services.AddSingleton(provider => _dbContext);

            JSInterop.Mode = JSRuntimeMode.Loose;
            JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);

            _popoverProvider = RenderComponent<MudPopoverProvider>();
        }

        [Fact]
        public async Task Loads_component()
        {
            // Arrange
            var component = RenderComponent<QueryBuilder<MyDbContext, Person>>();
            // Act
            // Assert
            component.Should().NotBeNull();
        }
    }
}
