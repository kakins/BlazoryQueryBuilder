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

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public async Task<ActionResult<IEnumerable>> Post([FromBody] Predicate predicate)
        {

            var queryService = _queryServiceFactory.Create(predicate.EntityType);
            IEnumerable data = await queryService.QueryData(predicate.LambdaExpression);

            return Ok(data);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
