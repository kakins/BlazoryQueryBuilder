using BlazoryQueryBuilder.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

public static class QueryBuilderServicesExtensions
{
    public static IServiceCollection AddQueryBuilderServices<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
    {
        services.AddTransient(typeof(QueryBuilderService<>));
        services.AddSingleton<PredicateFactory>();
        
        services.AddTransient<QueryServiceFactory<TDbContext>>();
        services.AddDbContext<TDbContext>(options =>
        {
            options.UseInMemoryDatabase("InMemoryDb");
            options.UseLazyLoadingProxies();
        });

        var dbContextType = typeof(TDbContext);
        var dbSetProperties = dbContextType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

        foreach (var dbSetProperty in dbSetProperties)
        {
            var entityType = dbSetProperty.PropertyType.GetGenericArguments()[0];
            var queryServiceType = typeof(QueryService<,>).MakeGenericType(entityType, dbContextType);
            services.AddTransient(queryServiceType);
        }

        return services;
    }
}
