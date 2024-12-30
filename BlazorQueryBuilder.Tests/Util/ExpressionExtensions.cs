using System.Collections.Generic;
using System.Linq.Expressions;

namespace BlazorQueryBuilder.Tests.Util
{
    public static class ExpressionExtensions
    {
        public static IEnumerable<string> GetMemberExpressionMembers(this MemberExpression memberExpression)
        {
            var members = new List<string>();
            var currentExpression = memberExpression;
            while (currentExpression != null)
            {
                members.Add(currentExpression.Member.Name);
                currentExpression = currentExpression.Expression as MemberExpression;
            }
            members.Reverse();
            return members;
        }
    }
}
