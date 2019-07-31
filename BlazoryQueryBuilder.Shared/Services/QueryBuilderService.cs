using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using BlazoryQueryBuilder.Shared.Extensions;

namespace BlazoryQueryBuilder.Shared.Services
{

    public class QueryBuilderService<TEntity>
    {
        public LambdaExpression Lambda { get; set; }
        public ParameterExpression Parameter { get; set; }

        public void LoadEntity()
        {
            Parameter = Expression.Parameter(
                typeof(TEntity),
                typeof(TEntity).Name.ToLower());

            Lambda = new PredicateFactory().CreateRelationalPredicate<TEntity>(
                typeof(TEntity).GetProperties().First().Name,
                Parameter,
                typeof(TEntity).GetProperties().First().PropertyType.GetDefaultValue(),
                ExpressionType.Equal);
        }

        public void LoadQuery(string expression)
        {
            var config = new ParsingConfig { RenameParameterExpression = true };

            Lambda = DynamicExpressionParser.ParseLambda(
                config,
                typeof(TEntity),
                typeof(bool),
                expression);

            Parameter = Lambda.Parameters[0];
        }

    }
}
