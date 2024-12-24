using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlazoryQueryBuilder.Shared.Services
{
    public class QueryServiceFactory<TDbContext> where TDbContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;

        public QueryServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IQueryService Create(string typeName)
        {
            var assembly = typeof(QueryBuilderService<>).Assembly;
            var type = assembly.GetType(typeName);

            var service = _serviceProvider.GetRequiredService(typeof(QueryService<,>).MakeGenericType(type, typeof(TDbContext)));

            return service as IQueryService;
        }
    }
}