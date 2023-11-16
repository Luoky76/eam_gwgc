using Chloe;
using Chloe.DbExpressions;
using Chloe.RDBMS;
using System.Reflection;

namespace Gksyb.Common.Data
{
    public class Compare_Handler : IMethodHandler
    {
        public static readonly string MethodName = "Compare";
        private static readonly MethodInfo Compare = typeof(string).GetMethod(MethodName, new Type[] { typeof(string), typeof(string) });

        public bool CanProcess(DbMethodCallExpression exp) => exp.Method == Compare && exp.Arguments?.Count == 2;

        public void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            //(case when left = right then 0 when left > right then 1 else -1 end)
            var left = exp.Arguments[0];
            var right = exp.Arguments[1];
            var equalPair = new DbCaseWhenExpression.WhenThenExpressionPair(new DbEqualExpression(left, right), DbConstantExpression.Zero);
            var greaterThanPair = new DbCaseWhenExpression.WhenThenExpressionPair(new DbGreaterThanExpression(left, right), DbConstantExpression.One);
            var caseWhenEqualExpression = DbExpression.CaseWhen(new List<DbCaseWhenExpression.WhenThenExpressionPair>(1)
            {
                equalPair,
                greaterThanPair
            }, new DbConstantExpression(-1), PublicConstants.TypeOfBoolean);
            caseWhenEqualExpression.Accept(generator);
        }
    }
}