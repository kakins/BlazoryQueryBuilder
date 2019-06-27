using BlazoryQueryBuilder.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazorQueryBuilder.Tests.Util
{
    public class TestContext : DbContext
    {
        public DbSet<Person> Persons { get; set; }

        public TestContext(DbContextOptions<TestContext> options): base(options)
        {
            
        }
    }
}