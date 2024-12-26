using BlazorQueryBuilder.Tests.Util;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using FluentAssertions;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace BlazorQueryBuilder.Tests.ExpressionVisitors
{
    public class LambdaParseTests
    {
        private TestContext _textContext;
        private Mock<IServiceProvider> _serviceProvider;

        public LambdaParseTests()
        {
            _textContext = new TestContext(EfHelpers.CreateEfInMemoryContextOptions<TestContext>("TestContext"));
            _textContext.Persons.Add(new Person { PersonId = "1", FirstName = "Casey", LastName = "Jones" });
            _textContext.SaveChanges();
            _serviceProvider = new Mock<IServiceProvider>();
            _serviceProvider
                .Setup(provider => provider.GetService(typeof(QueryService<Person, TestContext>)))
                .Returns(new QueryService<Person, TestContext>(_textContext));
        }

        [Fact]
        public async System.Threading.Tasks.Task ParseLambdaAndReturnData()
        {
            // Arrange
            Expression<Func<Person, bool>> expression = person => person.PersonId == "1" && person.LastName == "Jones";
            var properties = new List<string> { nameof(Person.PersonId), nameof(Person.FirstName), nameof(Person.LastName) };

            // Act
            var service = new QueryServiceFactory<TestContext>(_serviceProvider.Object)
                .Create<Person>();
            var data = await service.QueryData(expression, properties);

            // Assert
            data.Should().BeEquivalentTo(new List<Person> { new() { PersonId = "1", FirstName = "Casey", LastName = "Jones" } });
        }

        [Fact]
        public void ParseSelectLambdaFromProperties()
        {
            string[] properties = { nameof(Person.PersonId), nameof(Person.FirstName) };

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
