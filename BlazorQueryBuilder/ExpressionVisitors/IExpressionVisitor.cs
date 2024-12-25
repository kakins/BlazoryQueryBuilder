using System.Linq.Expressions;

namespace BlazorQueryBuilder.ExpressionVisitors
{
    public interface IExpressionVisitor<T> where T : Expression
    {
        T Execute();
    }
}
