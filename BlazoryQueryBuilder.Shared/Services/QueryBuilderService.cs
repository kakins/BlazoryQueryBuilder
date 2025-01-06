using BlazoryQueryBuilder.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Linq.Expressions;

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
            config.CustomTypeProvider = new CustomEFTypeProvider(config, true);

            Lambda = DynamicExpressionParser.ParseLambda(
                config,
                typeof(TEntity),
                typeof(bool),
                expression);

            Parameter = Lambda.Parameters[0];
        }
    }

    public class CustomEFTypeProvider : DefaultDynamicLinqCustomTypeProvider
    {
        public CustomEFTypeProvider(ParsingConfig config, bool cache) : base(config, cache)
        {
        }

        public override HashSet<Type> GetCustomTypes()
        {
            var customTypes = base.GetCustomTypes();

            // All three types are required to get dynamic access to EF.Functions
            customTypes.Add(typeof(EF));
            customTypes.Add(typeof(DbFunctions));
            customTypes.Add(typeof(DbFunctionsExtensions));
            
            return customTypes;
        }
    }
}
