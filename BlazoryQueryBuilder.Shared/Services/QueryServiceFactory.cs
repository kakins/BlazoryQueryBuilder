using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
            typeName = $"{nameof(BlazoryQueryBuilder)}.Shared.Models.{typeName}";
            Assembly assembly = typeof(QueryBuilderService<>).Assembly;
            Type type = assembly.GetType(typeName);

            var service = _serviceProvider.GetRequiredService(typeof(QueryService<,>).MakeGenericType(type, typeof(TDbContext)));

            return service as IQueryService;
        }
    }
}