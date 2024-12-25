using BlazoryQueryBuilder.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorQueryBuilder
{
    public class MyDbContext : DbContext
    {
        public DbSet<Person> Persons { get; set; }
        public DbSet<Address> Addresses { get; set; }

        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Person>()
                .ToTable(nameof(Person))
                .HasMany(person => person.Addresses)
                .WithOne(address => address.Person)
                .HasForeignKey(address => address.PersonId);
        }
    }

    public static class MyDbContextExtensions
    {
        public static async Task SeedDatabase(this MyDbContext context)
        {
            if (!context.Persons.Any())
            {
                var persons = new List<Person>
                {
                    new Person
                    {
                        FirstName = "Alice",
                        LastName = "Jones",
                        PersonId = "1",
                        Created = DateTime.Now
                    },
                    new Person
                    {
                        FirstName = "Bob",
                        LastName = "Smith",
                        PersonId = "2",
                        Created = DateTime.Parse("2021-01-01")
                    }
                };

                context.Persons.AddRange(persons);
                await context.SaveChangesAsync();

                var addresses = new List<Address>
                {
                    new Address
                    {
                        AddressId = 1,
                        PersonId = "1",
                        IsPrimary = true
                    },
                    new Address
                    {
                        AddressId = 2,
                        PersonId = "2",
                        IsPrimary = false
                    }
                };

                context.Addresses.AddRange(addresses);
                await context.SaveChangesAsync();
            }
        }
    }
}
