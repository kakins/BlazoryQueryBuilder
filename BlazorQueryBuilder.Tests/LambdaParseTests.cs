using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
            Expression<Func<Person, bool>> expression = person => person.PersonId == "1" && person.LastName == "Jones";
            var properties = new List<string>{ nameof(Person.PersonId), nameof(Person.FirstName) };

            var predicate = new Predicate
            {
                EntityType = nameof(Person),
                LambdaExpression = expression.ToString(),
                SelectedProperties = properties
            };

            IQueryService service = new QueryServiceFactory<TestContext>(_serviceProvider.Object)
                .Create(predicate.EntityType);

            IEnumerable data = await service.QueryData(predicate.LambdaExpression, properties);
        }

        [Fact]
        public void ParseSelectLambdaFromProperties()
        {
            string[] properties = {nameof(Person.PersonId), nameof(Person.FirstName)};

            var builder = new SelectBuilderService<Person>()
                .BuildSelect(properties);

            Type createdType = typeof(Person);
            ParameterExpression personParm = Expression.Parameter(typeof(Person), nameof(Person).ToLower());
            NewExpression ctor = Expression.New(createdType);

            var memberBindings = new List<MemberBinding>();

            foreach (string property in properties)
            {
                PropertyInfo prop = createdType.GetProperty(property);
                MemberExpression parmProp = Expression.MakeMemberAccess(personParm, prop);
                MemberAssignment assignment = Expression.Bind(prop, parmProp);

                memberBindings.Add(assignment);
            }

            MemberInitExpression memberInit = Expression.MemberInit(ctor, memberBindings);

            Expression<Func<Person, Person>> lambda = Expression.Lambda<Func<Person, Person>>(memberInit, personParm);


        }
    }
}
