using Chloe;
using Chloe.DbExpressions;
using Chloe.RDBMS;
using Chloe.RDBMS.MethodHandlers;
using System.Reflection;

namespace Gksyb.Common.Data
{
    public class Compare_Handler : Compare_HandlerBase
    {
        public static readonly string MethodName = "Compare";
        private static readonly MethodInfo Compare = typeof(string).GetMethod(MethodName, new Type[] { typeof(string), typeof(string) });

        public override bool CanProcess(DbMethodCallExpression exp) => exp.Method.DeclaringType == PublicConstants.TypeOfSql || (exp.Method == Compare && exp.Arguments?.Count == 2);

        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            if (exp.Method.DeclaringType == PublicConstants.TypeOfSql)
            {
                base.Process(exp, generator);
                return;
            }
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