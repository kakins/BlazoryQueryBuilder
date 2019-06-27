using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorQueryBuilder.Tests.Util
{
    public class EfHelpers
    {
        public static DbContextOptions<T> CreateEfInMemoryContextOptions<T>(string dbName) where T : DbContext
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var builder = new DbContextOptionsBuilder<T>()
                .UseInMemoryDatabase(dbName)
                .UseInternalServiceProvider(serviceProvider);

            return builder.Options;
        }
    }
}