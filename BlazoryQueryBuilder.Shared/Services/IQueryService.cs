using System.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace BlazoryQueryBuilder.Shared.Services
{
    public interface IQueryService
    {
        Task<IEnumerable> QueryData(string expression);
    }
}