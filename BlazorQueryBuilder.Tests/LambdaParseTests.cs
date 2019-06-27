using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using BlazorQueryBuilder.Tests.Util;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using Moq;
using Xunit;

namespace BlazorQueryBuilder.Tests
{
    public class LambdaParseTests
    {
        private TestContext _textContext;
        private Mock<IServiceProvider> _serviceProvider;

        public LambdaParseTests()
        {
            _textContext = new TestContext(EfHelpers.CreateEfInMemoryContextOptions<TestContext>("TestContext"));
            _textContext.Persons.Add(new Person {PersonId = "1", LastName = "Jones"});
            _textContext.SaveChanges();
            _serviceProvider = new Mock<IServiceProvider>();
            _serviceProvider
                .Setup(provider => provider.GetService(typeof(QueryService<Person, TestContext>)))
                .Returns(new QueryService<Person, TestContext>(_textContext));
        }

        [Fact]
        public async void ParseLambdaAndReturnData()
        {
            Expression<Func<Person, bool>> expression = worker => worker.PersonId == "1" && worker.LastName == "Jones";

            var predicate = new Predicate
            {
                EntityType = nameof(Person),
                LambdaExpression = expression.ToString()
            };

            IQueryService service = new QueryServiceFactory<TestContext>(_serviceProvider.Object)
                .Create(predicate.EntityType);

            IEnumerable data = await service.QueryData(predicate.LambdaExpression);
        }
    }
}
