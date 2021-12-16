using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlazoryQueryBuilder.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly QueryServiceFactory<MyDbContext> _queryServiceFactory;

        public ValuesController(QueryServiceFactory<MyDbContext> queryServiceFactory)
        {
            _queryServiceFactory = queryServiceFactory;
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable>> Post([FromBody] Predicate predicate)
        {
            IQueryService queryService = _queryServiceFactory.Create(predicate.EntityType);
            IEnumerable data = await queryService.QueryData(predicate.LambdaExpression, predicate.SelectedProperties);

            return Ok(data);
        }
    }
}
