using Bunit;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorQueryBuilder.Tests.Util
{
    public static class BUnitExtensions
    {
        public static IRenderedComponent<TInput> FindInputByLabel<TInput, TType>(
            this IRenderedFragment component, string label)
            where TInput : MudBaseInput<TType>
        {
            return component
                .FindComponents<TInput>()
                .FirstOrDefault(select => select.Instance.Label == label);
        }
    }
}
