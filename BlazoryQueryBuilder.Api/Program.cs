using BlazoryQueryBuilder.Api;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy",
        b => b.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
});

builder.Services.AddTransient<QueryServiceFactory<MyDbContext>>();
builder.Services.AddTransient<QueryService<Person, MyDbContext>>();
builder.Services.AddDbContext<MyDbContext>(options =>
{
    options.UseInMemoryDatabase("InMemoryDb");
    options.UseLazyLoadingProxies();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseDeveloperExceptionPage();
    //var dbContext = app.Services.GetRequiredService<MyDbContext>();
    //dbContext.Database.EnsureCreated();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("MyPolicy");
app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();
SeedDatabase(app.Services);

app.Run();

static void SeedDatabase(IServiceProvider services)
{
    using (var scope = services.CreateScope())
    {
        var provider = scope.ServiceProvider;
        //var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

        try
        {
            var aspnetRunContext = provider.GetRequiredService<MyDbContext>();
            MyDbContextSeed.SeedAsync(aspnetRunContext/*, loggerFactory*/).Wait();
        }
        catch (Exception exception)
        {
            //var logger = loggerFactory.Create("Main");
            //logger.Error("An error occurred seeding the DB.", exception);
        }
    }
}

public class MyDbContextSeed
{
    public static async Task SeedAsync(MyDbContext context/*, ILoggerFactory loggerFactory*/, int? retry = 0)
    {
        int retryForAvailability = retry.Value;

        try
        {
            // TODO: Only run this if using a real database
            // aspnetrunContext.Database.Migrate();
            // aspnetrunContext.Database.EnsureCreated();

            if (!context.Persons.Any())
            {
                context.Persons
                    .AddRange(
                        new Person
                        {
                            FirstName = "Alice",
                            LastName = "Jones",
                            PersonId = "1",
                            Addresses = new List<Address>()
                        },
                        new Person
                        {
                            FirstName = "Bob",
                            LastName = "Smith",
                            PersonId = "2",
                            Addresses = new List<Address>()
                        },
                        new Person
                        {
                            FirstName = "Celal",
                            LastName = "Ak",
                            PersonId = "3",
                            Addresses = new List<Address>()
                        }
                    );
                await context.SaveChangesAsync();
            }
        }
        catch (Exception exception)
        {
            if (retryForAvailability < 10)
            {
                retryForAvailability++;
                //var log = loggerFactory.Create("AspnetRunContextSeed");
                //log.Error(exception.Message);
                await SeedAsync(context/*, loggerFactory*/, retryForAvailability);
            }
            throw;
        }
    }
}