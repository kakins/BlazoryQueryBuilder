using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace BlazoryQueryBuilder.Shared.Services
{
    public interface IQueryServiceFactory<TDbContext> where TDbContext : DbContext
    {
        IQueryService<T> Create<T>() where T : class;
    }

    public class QueryServiceFactory<TDbContext> : IQueryServiceFactory<TDbContext> 
        where TDbContext : DbContext
    {
        private readonly IServiceProvider _serviceProvider;

        public QueryServiceFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IQueryService<T> Create<T>() where T : class
        {
            var service = _serviceProvider.GetRequiredService(typeof(QueryService<,>).MakeGenericType(typeof(T), typeof(TDbContext)));

            return service as IQueryService<T>;
        }
    }
}