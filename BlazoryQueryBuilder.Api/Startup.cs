using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BlazoryQueryBuilder.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();//.AddJsonOptions(options => { options.JsonSerializerOptions.PropertyNamingPolicy = null; });

            services.AddCors(options =>
            {
                options.AddPolicy("MyPolicy",
                    b => b.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
            });
            services.AddTransient<QueryServiceFactory<MyDbContext>>();
            services.AddTransient<QueryService<Person, MyDbContext>>();
            services.AddDbContext<MyDbContext>(options =>
            {
                options.UseInMemoryDatabase("InMemoryDb");
                options.UseLazyLoadingProxies();
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider provider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                var dbContext = provider.GetRequiredService<MyDbContext>();
                dbContext.Database.EnsureCreated();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseCors("MyPolicy");
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public static void SeedDatabase(IServiceProvider services)
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
}
