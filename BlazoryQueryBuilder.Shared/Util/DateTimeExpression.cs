using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BlazoryQueryBuilder.Shared.Util
{
    public static class DateTimeExpression
    {
        public static NewExpression New(DateTime dateTime)
        {
            var ticksExpression = Expression.Constant(dateTime.Ticks, typeof(long));
            return Expression.New(typeof(DateTime).GetConstructor([typeof(long)]), ticksExpression);
        }
    }
}
