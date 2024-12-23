using BlazoryQueryBuilder.Shared.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Hosting;
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

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            //app.AddComponent<App>("app");
            //app.ConfigureUi(env);
        }
        private static void ConfigureUi(WebAssemblyHostBuilder builder)
        {
            builder.RootComponents.Add<App>("#ApplicationContainer");
        }
    }
}
