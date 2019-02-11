using BlazorQueryBuilder.Models;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace BlazorQueryBuilder
{

    public class QueryBuilderService<TEntity>
    {
        public LambdaExpression Lambda { get; set; }
        public ParameterExpression Parameter { get; set; }
        //public Type SelectedEntityType { get; set; }

        public void SelectEntity(string entityTypeName)
        {
            var typeName = $"BlazorQueryBuilder.Models.{(string)entityTypeName}";
            Assembly assembly = typeof(Startup).Assembly;
            //SelectedEntityType = assembly.GetType(typeName);
        }

        public void LoadEntity()
        {
            Parameter = Expression.Parameter(
                typeof(TEntity),
                typeof(TEntity).Name.ToLower());

            Lambda = new PredicateFactory().CreateRelationalLambda<TEntity>(
                typeof(TEntity).GetProperties().First().Name,
                Parameter,
                typeof(TEntity).GetProperties().First().PropertyType.GetDefaultValue(),
                ExpressionType.Equal);

            //SelectedEntityType = typeof(TEntity);
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

            //SelectedEntityType = typeof(TEntity);
        }
    }
}
