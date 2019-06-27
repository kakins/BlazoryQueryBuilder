using System;
using Microsoft.AspNetCore.Components;

namespace BlazorQueryBuilder.Pages
{
    public class QueryBuilderBase: ComponentBase
    {
        public void DoSomething()
        {
            Console.WriteLine("Did something");
        }
    }
}