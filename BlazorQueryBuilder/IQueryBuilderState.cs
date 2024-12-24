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
