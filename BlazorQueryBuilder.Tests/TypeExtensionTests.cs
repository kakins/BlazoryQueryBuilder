using BlazoryQueryBuilder.Shared.Util;
using System.Reflection;
using Xunit;

namespace BlazorQueryBuilder.Tests
{
    public class TypeExtensionTests
    {
        [Fact]
        public void Enumerable_GetMethod_Any()
        {
            MethodInfo anyMethod = EnumerableMethodInfo.Any<int>();

            Assert.NotNull(anyMethod);
            Assert.True(anyMethod.IsGenericMethod);
        }

        [Fact]
        public void Enumerable_GetMethod_Select()
        {
            MethodInfo selectMethod = EnumerableMethodInfo.Select<int, int>();

            Assert.NotNull(selectMethod);
            Assert.True(selectMethod.IsGenericMethod);
        }
    }
}
