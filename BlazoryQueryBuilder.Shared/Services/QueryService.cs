using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;

namespace BlazoryQueryBuilder.Shared.Services
{
    public class QueryService<T, TDbContext> : IQueryService<T> where T : class where TDbContext : DbContext
    {
        private readonly TDbContext _dbContext;

        public QueryService(TDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable> QueryData(string predicateExpression, IEnumerable<string> selectedProperties)
        {
            try
            {
                predicateExpression = predicateExpression
                    .Replace("AndAlso", "&&")
                    .Replace("OrElse", "||");

                var options = ScriptOptions
                    .Default
                    .AddReferences(typeof(T).Assembly)
                    // TODO: Replace hardcoded namespace with the namespace of the DbContext
                    .AddImports("BlazoryQueryBuilder.Shared.Models", "System");

                Expression<Func<T, bool>> predicate = await CSharpScript.EvaluateAsync<Expression<Func<T, bool>>>(predicateExpression, options);

                return await QueryData(predicate, selectedProperties);

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<IEnumerable> QueryData(Expression<Func<T, bool>> predicate, IEnumerable<string> selectedProperties)
        {
            try
            {
                var select = new SelectBuilderService<T>().BuildSelect(selectedProperties);
                
                var results = await _dbContext
                    .Set<T>()
                    .Where(predicate)
                    .Select(select)
                    .ToListAsync();
                
                return results.AsEnumerable();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}