using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace BlazoryQueryBuilder.Shared.Services
{
    public class SelectBuilderService<TEntity>
    {
        public Expression<Func<TEntity, TEntity>> BuildSelect(IEnumerable<string> propertyNames)
        {
            Type entityType = typeof(TEntity);
            ParameterExpression entityParam = Expression.Parameter(typeof(TEntity), typeof(TEntity).Name.ToLower());
            NewExpression newEntity = Expression.New(entityType);

            var entityBindings = new List<MemberBinding>();

            foreach (string propertyName in propertyNames)
            {
                PropertyInfo property = entityType.GetProperty(propertyName);
                MemberExpression memberAccess = Expression.MakeMemberAccess(entityParam, property);
                MemberAssignment memberAssignment = Expression.Bind(property, memberAccess);
                entityBindings.Add(memberAssignment);
            }

            MemberInitExpression entityMemberInit = Expression.MemberInit(newEntity, entityBindings);

            Expression<Func<TEntity, TEntity>> lambda = Expression.Lambda<Func<TEntity, TEntity>>(entityMemberInit, entityParam);

            return lambda;
        }
    }
}
