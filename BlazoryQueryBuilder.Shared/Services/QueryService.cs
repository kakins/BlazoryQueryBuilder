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
    public class QueryService<T, TDbContext> : IQueryService where T : class where TDbContext : DbContext
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
                    .AddImports("BlazoryQueryBuilder.Shared.Models");

                Expression<Func<T, bool>> predicate = await CSharpScript.EvaluateAsync<Expression<Func<T, bool>>>(predicateExpression, options);

                Expression<Func<T, T>> select = new SelectBuilderService<T>().BuildSelect(selectedProperties);

                // create 
                IEnumerable<T> results = _dbContext
                    .Set<T>()
                    .Where(predicate)
                    .Select(select)
                    .ToList();

                return results.AsEnumerable();

            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}