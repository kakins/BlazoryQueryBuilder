using BlazorQueryBuilder.Pages;
using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.RenderTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazorQueryBuilder
{
    public interface IQueryBuilderState
    {
        IQueryBuilderState DisplayBuilder(Action onDisplay);
    }

    public class New : IQueryBuilderState
    {
        public IQueryBuilderState DisplayBuilder(Action onDisplay)
        {
            onDisplay();
            return this;
        }
    }


    public class Loaded: IQueryBuilderState
    {
        public IQueryBuilderState DisplayBuilder(Action onDisplay)
        {
            onDisplay();
            return this;
        }

    }

    public class None: IQueryBuilderState
    {
        public IQueryBuilderState DisplayBuilder(Action onDisplay)
        {
            return this;
        }
    }
}
