using BlazorQueryBuilder;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

try
{
    Console.WriteLine("Hello World!");
    var builder = WebApplication.CreateBuilder(args);

    builder.Services
        .AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddMudServices();

    builder.Services.AddTransient<QueryServiceFactory<MyDbContext>>();
    builder.Services.AddTransient<QueryService<Person, MyDbContext>>();
    builder.Services.AddDbContext<MyDbContext>(options =>
    {
        options.UseInMemoryDatabase("InMemoryDb");
        options.UseLazyLoadingProxies();
    });
    builder.Services.AddSingleton<PredicateFactory>();
    builder.Services.AddTransient(typeof(QueryBuilderService<>));

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseAntiforgery();

    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    using var scope = app.Services.CreateScope();
    var provider = scope.ServiceProvider;
    var dbContext = provider.GetRequiredService<MyDbContext>();
    await dbContext.SeedDatabase();

    app.Run();
}
catch (Exception ex)
{

    throw;
}

static async Task SeedDatabase(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var provider = scope.ServiceProvider;
    var dbContext = provider.GetRequiredService<MyDbContext>();
    await dbContext.SeedDatabase();
}
