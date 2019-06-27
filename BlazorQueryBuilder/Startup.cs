using System.Collections.Generic;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorQueryBuilder
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<PredicateFactory>();
            services.AddTransient(typeof(QueryBuilderService<>));
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
