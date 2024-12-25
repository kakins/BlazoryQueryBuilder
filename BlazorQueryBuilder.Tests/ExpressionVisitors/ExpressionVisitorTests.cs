using BlazorQueryBuilder.ExpressionVisitors;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Util;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Xunit;

namespace BlazorQueryBuilder.Tests.ExpressionVisitors
{
    public class ExpressionVisitorTests
    {
        [Fact]
        public void ChangeBinaryToMethodCall()
        {
            // person.PersonId == "1"
            BinaryExpression personIdEqualsOne =
                Expression.MakeBinary(
                    ExpressionType.Equal,
                    Expression.MakeMemberAccess(
                        Expression.Parameter(typeof(Person), "person"),
                        typeof(Person).GetProperty(nameof(Person.PersonId))),
                    Expression.Constant("1"));

            // person.PersonId
            var personId = (MemberExpression)personIdEqualsOne.Left;

            // person.Addresses
            MemberExpression personAddresses = ExpressionVisitorFactory.ChangePropertyAccess(
                    personId,
                    typeof(Person),
                    nameof(Person.Addresses))
                .Execute();

            // Select method
            MethodInfo selectMethod = EnumerableMethodInfo.Select<Address, int>();

            // address
            ParameterExpression addressParam = Expression.Parameter(typeof(Address), "address");

            // address.AddressId
            MemberExpression memberAccess = Expression.MakeMemberAccess(
                addressParam,
                typeof(Address).GetProperty(nameof(Address.AddressId)));

            // address => address.AddressId
            LambdaExpression lambda = Expression.Lambda(
                memberAccess,
                addressParam
            );

            // person.Addresses.Select(address => address.AddressId)
            MethodCallExpression selectAddresses = Expression.Call(selectMethod, new List<Expression> { personAddresses, lambda });

            Assert.NotNull(selectAddresses);
        }
    }
}
