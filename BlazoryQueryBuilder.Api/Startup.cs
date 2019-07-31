using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            services.AddControllers();
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
    }

    public class MyDbContext : DbContext
    {
        public DbSet<Person> Persons { get; set; }
        private DbContextOptions<MyDbContext> _options;

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
            _options = options;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Person>()
                .ToTable(nameof(Person));

            if (_options.Extensions.Select(e => e.GetType()).Contains(typeof(InMemoryDbContextOptionsExtensions)))
            {
                modelBuilder
                    .Entity<Person>()
                    .HasData(
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
                        });

            }

            modelBuilder
                .Entity<Person>()
                .HasMany(person => person.Addresses)
                .WithOne(address => address.Person)
                .HasForeignKey(address => address.PersonId);

        }
    }
}
