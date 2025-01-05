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

            _lambdaExpression = ExpressionHelpers.CreateLambda<Person>(person => person.PersonId == "1");
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

        [Theory]
        [MemberData(nameof(SelectedFieldTestData))]
        public async Task Updates_selected_field(LambdaExpression lambdaExpression, List<string> fields)
        {
            // Arrange
            var predicateExpression = lambdaExpression.Body;
            var predicateParameter = lambdaExpression.Parameters[0];

            var component = CreateComponent(
                predicateExpression,
                predicateParameter,
                onChange: expression =>
                {
                    predicateExpression = expression;
                });

            // Act
            var fieldSelect = component.FindInputByLabel<MudSelect<string>, string>("Field");
            foreach (var property in fields)
            {
                await component.InvokeAsync(async () =>
                {
                    await fieldSelect.Instance.OpenMenu();
                    await fieldSelect.Instance.SelectOption(property);
                });
            }

            // Assert
            // Verify the operand member expression matches the property selections
            // Changing fields will always return a new binary expression
            predicateExpression
                .As<BinaryExpression>()
                .Left
                .As<MemberExpression>()
                .GetMemberNames()
                .Should().BeEquivalentTo(fields);
            fieldSelect
                .Instance.Value
                .Should()
                .Be(fields.Last());
        }

        [Fact]
        public async Task Selects_first_property_of_navigation_field()
        {
            // Arrange
            var lambdaExpression = ExpressionHelpers.CreateLambda<Address>(address => address.AddressId == 1);
            var predicateExpression = lambdaExpression.Body.As<BinaryExpression>();
            var predicateParameter = lambdaExpression.Parameters[0];

            var component = CreateComponent(
                predicateExpression,
                predicateParameter,
                onChange: expression => { predicateExpression = expression.As<BinaryExpression>(); });

            // Act
            var fieldSelect = component.FindInputByLabel<MudSelect<string>, string>("Field");
            await component.InvokeAsync(async () =>
            {
                await fieldSelect.Instance.OpenMenu();
                await fieldSelect.Instance.SelectOption(nameof(Address.Person));
            });

            // Assert
            var newMemberName = nameof(Address.Person.PersonId);
            var memberExpression = predicateExpression.Left.As<MemberExpression>();
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

        public static TheoryData<LambdaExpression, List<string>> SelectedFieldTestData()
        {
            var binaryLambdaExpression = ExpressionHelpers.CreateLambda<Address>(address => address.AddressId == 1);
            var methodCallLambdaExpression = ExpressionHelpers.CreateLambda<Address>(address => EF.Functions.Like(address.City, "%Metropolis%"));
            
            return new TheoryData<LambdaExpression, List<string>>() 
            { 
                { binaryLambdaExpression, [nameof(Address.PersonId)] },
                { binaryLambdaExpression, [nameof(Address.Person), nameof(Person.LastName)]},
                { binaryLambdaExpression, [nameof(Address.Utilities), nameof(List < Utility >.Count)] },
                { methodCallLambdaExpression, [nameof(Address.AddressId)] },
                { methodCallLambdaExpression, [nameof(Address.Person), nameof(Person.FirstName)] },
                { methodCallLambdaExpression, [nameof(Address.Utilities), nameof(List <Utility>.Count)] }
            };
        }
    }
}
