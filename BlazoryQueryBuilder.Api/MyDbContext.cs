using System;
using System.Collections.Generic;
using System.Linq;
using BlazoryQueryBuilder.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;

namespace BlazoryQueryBuilder.Api
{
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
                .ToTable(nameof(Person))
                .HasMany(person => person.Addresses)
                .WithOne(address => address.Person)
                .HasForeignKey(address => address.PersonId);

            if (_options.Extensions.Select(e => e.GetType()).Contains(typeof(InMemoryOptionsExtension)))
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
        }
    }
}
