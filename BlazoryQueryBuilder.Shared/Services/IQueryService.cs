using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BlazoryQueryBuilder.Shared.Services
{
    public interface IQueryService<T> where T: class
    {
        Task<IEnumerable> QueryData(string predicateExpression, IEnumerable<string> selectedProperties);
        Task<IEnumerable> QueryData(Expression<Func<T, bool>> predicate, IEnumerable<string> selectedProperties);
    }
}