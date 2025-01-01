using BlazorQueryBuilder.Pages;
using BlazorQueryBuilder.Tests.Util;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using Bunit;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace BlazorQueryBuilder.Tests.Pages
{
    public class RelationalPredicateFieldTests : TestContext
    {
        private readonly LambdaExpression _lambdaExpression;
        private readonly ParameterExpression _predicateParameter;
        private readonly Expression _predicateExpression;

        public RelationalPredicateFieldTests()
        {
            Services.AddSingleton<PredicateFactory>();

            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;
            JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);

            RenderComponent<MudPopoverProvider>();

            _lambdaExpression = GetLambdaExpression<Person>(person => person.PersonId == "1");
            _predicateParameter = _lambdaExpression.Parameters[0];
            _predicateExpression = _lambdaExpression.Body;
        }

        [Theory]
        [MemberData(nameof(FieldItemTestData))]
        public async Task Initializes_field_items(LambdaExpression lambdaExpression, string expectedFieldName)
        {
            // Arrange
            var component = CreateComponent(lambdaExpression.Body, lambdaExpression.Parameters[0]);

            // Act
            var fieldSelect = component.FindInputByLabel<MudSelect<string>, string>("Field");
            var fieldItems = fieldSelect.Instance.Items.ToList();
            await component.InvokeAsync(async () =>
            {
                await fieldSelect.Instance.OpenMenu();
                fieldItems = [.. fieldSelect.Instance.Items];
            });

            // Assert
            fieldSelect.Instance.Value.Should().Be(expectedFieldName);

            var expectedFieldItems = lambdaExpression.Parameters[0].Type.GetProperties().Select(p => p.Name);
            fieldItems
                .Select(i => i.Value)
                .Should()
                .BeEquivalentTo(expectedFieldItems);
        }

        // TODO: Test/handle method call expressions
        [Theory]
        [MemberData(nameof(SelectedFieldTestData))]
        public async Task Updates_selected_field(List<string> fields)
        {
            // Arrange
            var lambdaExpression = GetLambdaExpression<Address>(address => address.AddressId == 1);
            var predicateExpression = GetLambdaBodyExpression<BinaryExpression>(lambdaExpression);
            var predicateParameter = lambdaExpression.Parameters[0];

            var component = CreateComponent(
                predicateExpression,
                predicateParameter,
                onChange: expression =>
                {
                    predicateExpression = expression as BinaryExpression;
                });

            // Act
            var fieldSelect = component.FindInputByLabel<MudSelect<string>, string>("Field");

            await component.InvokeAsync(async () =>
            {
                foreach (var property in fields)
                {
                    await fieldSelect.Instance.OpenMenu();
                    await fieldSelect.Instance.SelectOption(property);
                }
            });

            // Assert
            // Verify the operand member expression matches the property selections
            var members = ((MemberExpression)predicateExpression.Left).GetMemberExpressionMembers();
            members.Should().BeEquivalentTo(fields);
            fieldSelect.Instance.Value.Should().Be(fields.Last());
        }

        [Fact]
        public async Task Selects_first_property_of_navigation_field()
        {
            // Arrange
            var lambdaExpression = GetLambdaExpression<Address>(address => address.AddressId == 1);
            var predicateExpression = GetLambdaBodyExpression<BinaryExpression>(lambdaExpression);
            var predicateParameter = lambdaExpression.Parameters[0];

            var component = CreateComponent(
                predicateExpression,
                predicateParameter,
                onChange: expression => { predicateExpression = expression as BinaryExpression; });

            // Act
            var fieldSelect = component.FindInputByLabel<MudSelect<string>, string>("Field");
            await component.InvokeAsync(async () =>
            {
                await fieldSelect.Instance.OpenMenu();
                await fieldSelect.Instance.SelectOption(nameof(Address.Person));
            });

            // Assert
            var newMemberName = nameof(Address.Person.PersonId);
            var memberExpression = ((MemberExpression)predicateExpression.Left);
            memberExpression.Member.DeclaringType.Should().Be(typeof(Person));
            memberExpression.Member.Name.Should().Be(newMemberName);
            fieldSelect.Instance.Value.Should().Be(newMemberName);
        }

        private IRenderedComponent<RelationalPredicateField> CreateComponent(
            Expression predicateExpression = null,
            ParameterExpression parameterExpression = null,
            Action<Expression> onChange = null,
            Action<string> onNavigationPathChanged = null)
        {
            onChange ??= (_) => { };
            onNavigationPathChanged ??= (_) => { };

            return RenderComponent<RelationalPredicateField>(parameters =>
            {
                parameters
                    .Add(p => p.PredicateExpression, predicateExpression ?? _predicateExpression)
                    .Add(p => p.ParameterExpression, parameterExpression ?? _predicateParameter)
                    .Add(p => p.OnFieldChanged, onChange)
                    .Add(p => p.OnNavigatePathChanged, onNavigationPathChanged);
            });
        }

        public static TheoryData<LambdaExpression, string> FieldItemTestData()
        {
            Expression<Func<Person, bool>> binaryLambda = person => person.PersonId == "1";
            Expression<Func<Person, bool>> methodCallLambda = p => EF.Functions.Like(p.FirstName, "%Alice%");

            return new()
            {
                { binaryLambda, nameof(Person.PersonId) },
                { methodCallLambda, nameof(Person.FirstName) }
            };
        }

        public static TheoryData<List<string>> SelectedFieldTestData() =>
        [
            // address.PersonId
            [nameof(Address.PersonId)],
            // address.Person.LastName
            [nameof(Address.Person), nameof(Person.LastName)],
            // address.Utilities.Count
            [nameof(Address.Utilities), nameof(List<Utility>.Count)]
        ];

        private LambdaExpression GetLambdaExpression<T>(Expression<Func<T, bool>> lambdaExpression) => lambdaExpression;
        private T GetLambdaBodyExpression<T>(LambdaExpression lambdaExpression) where T : Expression => lambdaExpression.Body as T;
    }
}
