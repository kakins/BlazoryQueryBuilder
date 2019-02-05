using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Metadata;
using BlazorQueryBuilder;
using BlazorQueryBuilder.Models;
using BlazorQueryBuilder.Visitors;
using Xunit;

namespace BlazorTest.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var parameter = Expression.Parameter(typeof(Worker));

            var lambda = new PredicateFactory().CreateRelationalLambda<Worker>(
                typeof(Worker).GetProperties().First().Name,
                parameter,
                string.Empty,
                ExpressionType.Equal);

            var addLogicalBinary = new AddLogicalBinaryVisitor();
            var newLambda = (LambdaExpression)addLogicalBinary.Add(lambda);           
        }

        [Fact]
        public void ChangeMemberAccess()
        {
            MemberExpression workerUin = Expression.MakeMemberAccess(
                Expression.Parameter(typeof(Worker)),
                typeof(Worker).GetProperty(nameof(Worker.Uin)));

            Expression workerEmployeeId = new ChangeMemberProperty(typeof(Worker),
                    workerUin, 
                    nameof(Worker.EmployeeId))
                .Change();

            Assert.IsAssignableFrom<MemberExpression>(workerEmployeeId);
            Assert.Equal(nameof(Worker.EmployeeId), ((MemberExpression)workerEmployeeId).Member.Name);
        }

        [Fact]
        public void ChangeNestedMemberAccess()
        {
            MemberExpression workerUin = Expression.MakeMemberAccess(
                Expression.Parameter(typeof(Worker)),
                typeof(Worker).GetProperty(nameof(Worker.Uin)));

            MemberExpression workerPinOccupant = new ChangeMemberProperty(
                    typeof(Worker),
                    workerUin,
                    nameof(Worker.PinOccupant))
                .Change();

            MemberExpression newMember = Expression.MakeMemberAccess(
                workerPinOccupant,
                workerPinOccupant.Type.GetProperty(nameof(PinOccupant.PinOccupantId)));

            Assert.IsAssignableFrom<MemberExpression>(newMember);
            Assert.Equal(nameof(PinOccupant.PinOccupantId), newMember.Member.Name);
        }

        [Fact]
        public void ChangeBinaryRight()
        {
            ConstantExpression left = Expression.Constant(true);
            ConstantExpression right = Expression.Constant(true);

            BinaryExpression binary = Expression.MakeBinary(ExpressionType.Equal, left, right);

            BinaryExpression newBinary = new ReplaceBinaryRight(binary, Expression.Constant(false)).Replace();

            Assert.IsAssignableFrom<BinaryExpression>(newBinary);
            Assert.True(((ConstantExpression) newBinary.Right).Value.Equals(false));
        }

        [Fact]
        public void ChangeBinaryLeft()
        {
            ConstantExpression left = Expression.Constant(true);
            ConstantExpression right = Expression.Constant(true);

            BinaryExpression binary = Expression.MakeBinary(ExpressionType.Equal, left, right);

            BinaryExpression newBinary = new ReplaceBinaryLeft(binary, Expression.Constant(false)).Replace();

            Assert.IsAssignableFrom<BinaryExpression>(newBinary);
            Assert.True(((ConstantExpression)newBinary.Left).Value.Equals(false));
        }

        [Fact]
        public void ReplaceLambdaBody()
        {
            Expression<Func<Worker, bool>> originalLambda = worker => worker.Uin == "820009398";

            BinaryExpression newBody = new ReplaceBinaryType((BinaryExpression) originalLambda.Body, ExpressionType.NotEqual).Replace();

            LambdaExpression newLambda = new ReplaceLambdaBody(originalLambda, newBody).Replace();

            Assert.Equal(newLambda.Body, newBody);
        }

        [Fact]
        public void ChangeBinary()
        {
            BinaryExpression binary1 = Expression.MakeBinary(
                ExpressionType.Equal, 
                Expression.Constant(1), 
                Expression.Constant(1));

            BinaryExpression binary2 = Expression.MakeBinary(
                ExpressionType.Equal,
                Expression.Constant(2),
                Expression.Constant(2));

            BinaryExpression newBinary = new ReplaceBinary(binary1, binary2).Replace();

            Assert.Equal(newBinary, binary2);
        }
    }
}
