using System.Collections;
using System.Threading.Tasks;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazoryQueryBuilder.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class ValuesController : ControllerBase
    {
        private readonly QueryServiceFactory<MyDbContext> _queryServiceFactory;

        public ValuesController(QueryServiceFactory<MyDbContext> queryServiceFactory)
        {
            _queryServiceFactory = queryServiceFactory;
        }

        [HttpPost]
        public async Task<IEnumerable> Post([FromBody] Predicate predicate)
        {
            IQueryService queryService = _queryServiceFactory.Create(predicate.EntityType);
            IEnumerable data = await queryService.QueryData(predicate.LambdaExpression, predicate.SelectedProperties);

            return data;
        }

        [HttpGet]
        public async Task<IEnumerable> Get([FromBody] Predicate predicate)
        {
            IQueryService queryService = _queryServiceFactory.Create(predicate.EntityType);
            IEnumerable data = await queryService.QueryData(predicate.LambdaExpression, predicate.SelectedProperties);

            return data;
        }
    }
}
