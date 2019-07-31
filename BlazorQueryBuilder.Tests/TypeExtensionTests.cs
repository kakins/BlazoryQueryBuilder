using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using BlazoryQueryBuilder.Shared.Extensions;
using BlazoryQueryBuilder.Shared.Models;
using BlazoryQueryBuilder.Shared.Util;
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
