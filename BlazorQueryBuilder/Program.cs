using BlazorQueryBuilder;
using BlazoryQueryBuilder.Shared.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
        builder.Services.AddSingleton<PredicateFactory>();
        builder.Services.AddTransient(typeof(QueryBuilderService<>));

        //builder.Services.AddMsalAuthentication(options =>
        //{
        //    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
        //});

        await builder.Build().RunAsync();
    }
}