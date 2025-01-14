﻿using BlazorQueryBuilder.Pages;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using Bunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace BlazorQueryBuilder.Tests.Pages
{
    public class PredicateTests : TestContext
    {
        public PredicateTests()
        {
            Services.AddSingleton<PredicateFactory>();

            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;
            JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);

            RenderComponent<MudPopoverProvider>();
        }

        [Theory]
        [MemberData(nameof(RelationalPredicateTestData))]
        public async Task Initializes_relational_predicate(Expression<Func<Address, bool>> lambdaExpression)
        {
            // Arrange
            // Act
            var component = RenderComponent<BlazorQueryBuilder.Pages.Predicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, lambdaExpression.Body)
                    .Add(p => p.ParameterExpression, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Assert
            var logicalPredicate = component.FindComponent<RelationalPredicate>();
            logicalPredicate.Instance.PredicateExpression.Should().Be(lambdaExpression.Body);
            logicalPredicate.Instance.ParameterExpression.Should().Be(lambdaExpression.Parameters[0]);
        }

        [Theory]
        [MemberData(nameof(LogicalPredicateTestData))]
        public async Task Initializes_logical_predicate(Expression<Func<Person, bool>> lambdaExpression)
        {
            // Arrange
            // Act
            var component = RenderComponent<BlazorQueryBuilder.Pages.Predicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, lambdaExpression.Body.As<BinaryExpression>())
                    .Add(p => p.ParameterExpression, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, _ => { });
            });

            // Assert
            var logicalPredicate = component.FindComponent<LogicalPredicate>();
            logicalPredicate.Instance.PredicateExpression.Should().Be(lambdaExpression.Body.As<BinaryExpression>());
            logicalPredicate.Instance.ParameterExpression.Should().Be(lambdaExpression.Parameters[0]);
        }

        [Fact]
        public async Task Updates_parent_when_logical_predicate_changes()
        {
            // Arrange
            Expression<Func<Person, bool>> lambdaExpression = person => person.PersonId == "1" && person.PersonId == "2";
            var onChange = new Mock<Action<Expression>>();
            var component = RenderComponent<BlazorQueryBuilder.Pages.Predicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, lambdaExpression.Body.As<BinaryExpression>())
                    .Add(p => p.ParameterExpression, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, onChange.Object);
            });

            // Act
            var logicalPredicate = component.FindComponent<LogicalPredicate>();
            await logicalPredicate.InvokeAsync(() =>
            {
                logicalPredicate.Instance.OnChange.Invoke(lambdaExpression.Body.As<BinaryExpression>());
            });

            // Assert
            onChange.Verify(o => o.Invoke(lambdaExpression.Body), Times.Once);
        }

        [Fact]
        public async Task Updates_parent_when_relational_predicate_changes()
        {
            // Arrange
            Expression<Func<Person, bool>> lambdaExpression = person => person.PersonId == "1";
            var onChange = new Mock<Action<Expression>>();
            var component = RenderComponent<BlazorQueryBuilder.Pages.Predicate>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, lambdaExpression.Body)
                    .Add(p => p.ParameterExpression, lambdaExpression.Parameters[0])
                    .Add(p => p.OnChange, onChange.Object);
            });

            // Act
            var relationalPredicate = component.FindComponent<RelationalPredicate>();
            await relationalPredicate.InvokeAsync(() =>
            {
                relationalPredicate.Instance.OnChange.Invoke(lambdaExpression.Body);
            });

            // Assert
            onChange.Verify(o => o.Invoke(lambdaExpression.Body), Times.Once);
        }

        public static TheoryData<Expression<Func<Address, bool>>> RelationalPredicateTestData =>
        [
            address => address.AddressId == 1,
            address => address.AddressId != 1,
            address => address.AddressId > 1,
            address => address.AddressId >= 1,
            address => address.AddressId < 1,
            address => address.AddressId <= 1,
            address => EF.Functions.Like(address.City, "%a%"),  
            //address => address.City.StartsWith('a')
        ];

        public static TheoryData<Expression<Func<Person, bool>>> LogicalPredicateTestData =>
        [
            person => person.PersonId == "1" && person.PersonId == "2",
            person => person.PersonId == "1" || person.PersonId == "2",
        ];
    }
}
