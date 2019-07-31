using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazoryQueryBuilder.Shared.Services
{
    public interface IQueryService
    {
        Task<IEnumerable> QueryData(string predicateExpression, IEnumerable<string> selectedProperties);
    }
}