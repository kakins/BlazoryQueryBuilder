using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        //private readonly Type _type;

        public QueryService(TDbContext dbContext)
        {
            _dbContext = dbContext;
            //_type = type;
        }

        public async Task<IEnumerable> QueryData(string expression)
        {
            expression = expression.Replace("AndAlso", "&&").Replace("OrElse", "||");

            var options = ScriptOptions.Default.AddReferences(typeof(T).Assembly);

            Expression<Func<T, bool>> discountFilterExpression = await CSharpScript.EvaluateAsync<Expression<Func<T, bool>>>(expression, options);

            var lambda = PredicateFactory.GetLambda<T>(expression, typeof(T));
            IQueryable<T> results = _dbContext.Set<T>().Where(lambda);
            return results.AsEnumerable();
        }
    }
}