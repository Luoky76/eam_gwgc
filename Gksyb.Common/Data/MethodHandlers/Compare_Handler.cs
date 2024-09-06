using Chloe;
using Chloe.DbExpressions;
using Chloe.RDBMS;
using System.Reflection;

namespace Gksyb.Common.Data
{
    public class Compare_Handler : IMethodHandler
    {
        public const string MethodName = nameof(string.Compare);
        private static readonly MethodInfo method = typeof(string).GetMethod(MethodName, new Type[] { typeof(string), typeof(string) });

        public bool CanProcess(DbMethodCallExpression exp) => exp.Method.DeclaringType != PublicConstants.TypeOfSql && (exp.Method == method && exp.Arguments?.Count == 2);

        public void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            //(case when left = right then 0 when left > right then 1 else -1 end)
            var left = exp.Arguments[0];
            var right = exp.Arguments[1];
            var equalPair = new DbCaseWhenExpression.WhenThenExpressionPair(new DbEqualExpression(left, right), DbConstantExpression.Zero);
            var greaterThanPair = new DbCaseWhenExpression.WhenThenExpressionPair(new DbGreaterThanExpression(left, right), DbConstantExpression.One);
            var caseWhenEqualExpression = new DbCaseWhenExpression(PublicConstants.TypeOfBoolean, new List<DbCaseWhenExpression.WhenThenExpressionPair>(1)
            {
                equalPair,
                greaterThanPair
            }, new DbConstantExpression(-1));
            caseWhenEqualExpression.Accept(generator);
        }
    }
}