using BlazoryQueryBuilder.Shared.Util;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;

namespace BlazorQueryBuilder.Pages
{
    public partial class RelationalOperators
    {
        public static List<ExpressionOperator> GetOperators(Type operandType)
        {
            return operandType switch
            {
                Type type when type == typeof(int) => new()
                {
                    { new EqualsOperator() },
                    { new NotEqualsOperator() },
                    { new LessThanOperator() },
                    { new LessThanOrEqualOperator() },
                    { new GreaterThanOperator() },
                    { new GreaterThanOrEqualOperator() },
                    { new InListOperator<int>() },
                    { new InListOperator<int>(true) },
                },
                Type type when type == typeof(string) => new()
                {
                    { new EqualsOperator() },
                    { new NotEqualsOperator() },
                    { new EfLikeOperator() },
                    { new EfLikeOperator(true) },
                    { new InListOperator<string>() },
                    { new InListOperator<string>(true) },
                },
                Type type when type == typeof(bool) => new()
                {
                    { new EqualsOperator() },
                    { new NotEqualsOperator() },
                },
                Type type when type == typeof(DateTime) => new()
                {
                    { new EqualsOperator() },
                    { new NotEqualsOperator() },
                    { new LessThanOperator() },
                    { new LessThanOrEqualOperator() },
                    { new GreaterThanOperator() },
                    { new GreaterThanOrEqualOperator() },
                },
                _ => new()
                {
                    { new EqualsOperator() },
                    { new NotEqualsOperator() },
                },
            };
        }

        public class ExpressionOperator
        {
            public virtual ExpressionType ExpressionType { get; set; }
            public virtual string DisplayText { get; set; }

            public override bool Equals(object obj)
            {
                return obj is ExpressionOperator op
                    && ExpressionType == op.ExpressionType;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(ExpressionType);
            }

            public static bool operator ==(ExpressionOperator left, ExpressionOperator right)
            {
                return EqualityComparer<ExpressionOperator>.Default.Equals(left, right);
            }

            public static bool operator !=(ExpressionOperator left, ExpressionOperator right)
            {
                return !(left == right);
            }
        }

        public class BinaryOperator : ExpressionOperator { }

        public class EqualsOperator : BinaryOperator
        {
            public override ExpressionType ExpressionType => ExpressionType.Equal;
            public override string DisplayText => "Equals";
        }

        public class NotEqualsOperator : BinaryOperator
        {
            public override ExpressionType ExpressionType => ExpressionType.NotEqual;
            public override string DisplayText => "Does not equal";
        }

        public class LessThanOperator : BinaryOperator
        {
            public override ExpressionType ExpressionType => ExpressionType.LessThan;
            public override string DisplayText => "Less than";
        }

        public class LessThanOrEqualOperator : BinaryOperator
        {
            public override ExpressionType ExpressionType => ExpressionType.LessThanOrEqual;
            public override string DisplayText => "Less than or equal";
        }

        public class GreaterThanOperator : BinaryOperator
        {
            public override ExpressionType ExpressionType => ExpressionType.GreaterThan;
            public override string DisplayText => "Greater than";
        }

        public class GreaterThanOrEqualOperator : BinaryOperator
        {
            public override ExpressionType ExpressionType => ExpressionType.GreaterThanOrEqual;
            public override string DisplayText => "Greater than or equal";
        }

        public class MethodCallOperator : ExpressionOperator
        {
            public override ExpressionType ExpressionType => ExpressionType.Call;
            public MethodInfo MethodInfo { get; set; }
            public bool IsNegated { get; set; }

            public MethodCallOperator(MethodInfo methodInfo, bool isNegated = false)
            {
                MethodInfo = methodInfo;
                IsNegated = isNegated;
            }

            public override bool Equals(object obj)
            {
                return obj is MethodCallOperator op
                    && base.Equals(obj)
                    && EqualityComparer<MethodInfo>.Default.Equals(MethodInfo, op.MethodInfo)
                    && IsNegated == op.IsNegated;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(base.GetHashCode(), MethodInfo, IsNegated);
            }

            public static bool operator ==(MethodCallOperator left, MethodCallOperator right)
            {
                return EqualityComparer<MethodCallOperator>.Default.Equals(left, right);
            }

            public static bool operator !=(MethodCallOperator left, MethodCallOperator right)
            {
                return !(left == right);
            }
        }

        public class EfLikeOperator : MethodCallOperator
        {
            public override string DisplayText => IsNegated ? "Not like" : "Like";

            public EfLikeOperator(bool isNegated = false)
                : base(typeof(DbFunctionsExtensions).GetMethod("Like", new[] { typeof(DbFunctions), typeof(string), typeof(string) }), isNegated)
            {
            }
        }

        public class InListOperator<T> : MethodCallOperator
        {
            public override string DisplayText => IsNegated ? "Not in list" : "In list";
            public InListOperator(bool isNegated = false)
                : base(EnumerableMethodInfo.Contains<T>(), isNegated)
            {
            }
        }
    }
}
