using BlazoryQueryBuilder.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorQueryBuilder.Tests.Util
{
    public class TestDbContext : DbContext
    {
        public DbSet<Person> Persons { get; set; }

        public TestDbContext(DbContextOptions<TestDbContext> options): base(options)
        {
            
        }
    }
}