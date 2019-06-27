using System.Linq.Expressions;

namespace BlazoryQueryBuilder.Shared.Models
{
    public class Predicate
    {
        public string LambdaExpression { get; set; }
        public string EntityType { get; set; }
        public Expression Expression { get; set; }
    }
}