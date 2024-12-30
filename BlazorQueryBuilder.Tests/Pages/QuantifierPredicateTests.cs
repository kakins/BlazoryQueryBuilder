using BlazorQueryBuilder.ExpressionVisitors.Extensions;
using BlazorQueryBuilder.Pages;
using BlazorQueryBuilder.Tests.Util;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Services;
using BlazoryQueryBuilder.Shared.Util;
using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace BlazorQueryBuilder.Tests.Pages
{
    public class QuantifierPredicateTests : TestContext
    {
        private readonly LambdaExpression _lambdaExpression;
        private readonly ParameterExpression _predicateParameter;
        private readonly Expression _predicateExpression;

        public QuantifierPredicateTests()
        {
            Services.AddSingleton<PredicateFactory>();

            Services.AddMudServices();

            JSInterop.Mode = JSRuntimeMode.Loose;
            JSInterop.SetupVoid("mudKeyInterceptor.connect", _ => true);

            RenderComponent<MudPopoverProvider>();

            //_lambdaExpression = GetLambdaExpression(person => person.Addresses.Any(a => a.Id == 1));
            //_predicateParameter = _lambdaExpression.Parameters[0];
            //_predicateExpression = GetLambdaBodyExpression(_lambdaExpression);
        }

        // Test case for initializing a relational predicate with a Contains method
        // Description: This test should verify that the component correctly initializes a relational predicate that uses the Contains method on a user-defined list.
        // Example: new[] {1, 2}.Contains(person.PersonId)
        [Fact]
        public async Task Initializes_contains_method_call()
        {
            // TODO: Implement test
        }

        // Test case for updating the right operand when the Contains value changes
        // Description: This test should verify that the component correctly updates the right operand of a relational predicate when the value of the Contains method changes.
        // Example: new[] {1, 2}.Contains(person.PersonId) -> new[] {1, 2, 3}.Contains(person.PersonId)
        [Fact]
        public async Task Updates_right_operand_expression_when_contains_value_changes()
        {
            // TODO: Implement test
        }

        // Test case for initializing a quantifier predicate with Any method
        // Description: This test should verify that the component correctly initializes a quantifier predicate that uses the Any method with a condition.
        // Example: person.Addresses.Any(a => a.Id == 1)
        [Fact]
        public async Task Initializes_any_method_call()
        {
            // TODO: Implement test
        }

        // Test case for updating the right operand when the Any value changes
        // Description: This test should verify that the component correctly updates the right operand of a quantifier predicate when the value of the Any method changes.
        // Example: person.Addresses.Any(a => a.Id == 1) -> person.Addresses.Any(a => a.Id == 2)
        [Fact]
        public async Task Updates_right_operand_expression_when_any_value_changes()
        {
            // TODO: Implement test
        }

        // Test case for initializing a quantifier predicate with Any method and no arguments
        // Description: This test should verify that the component correctly initializes a quantifier predicate that uses the Any method without any arguments.
        // Example: person.Addresses.Any()
        [Fact]
        public async Task Initializes_any_exist_method_call()
        {
            // TODO: Implement test
        }

        // Test case for initializing a quantifier predicate with a complex Any method
        // Description: This test should verify that the component correctly initializes a quantifier predicate that uses the Any method with a complex condition.
        // Example: person.Addresses.Any(a => new[] {1, 2}.Contains(a.Id) && a.IsPrimary)
        [Fact]
        public async Task Initializes_complex_any_method_call()
        {
            // TODO: Implement test
        }

        // Test case for updating the right operand when the complex Any value changes
        // Description: This test should verify that the component correctly updates the right operand of a quantifier predicate when the value of the complex Any method changes.
        // Example: person.Addresses.Any(a => new[] {1, 2}.Contains(a.Id) && a.IsPrimary) -> person.Addresses.Any(a => new[] {1, 2, 3}.Contains(a.Id) && a.IsPrimary)
        [Fact]
        public async Task Updates_right_operand_expression_when_complex_any_value_changes()
        {
            // TODO: Implement test
        }

        private LambdaExpression GetLambdaExpression(Expression<Func<Person, bool>> lambdaExpression)
        {
            return lambdaExpression;
        }

        private Expression GetLambdaBodyExpression(LambdaExpression lambdaExpression)
        {
            return lambdaExpression.Body;
        }
    }
}
