using Chloe;
using Chloe.DbExpressions;
using Chloe.RDBMS;
using System.Reflection;

namespace Gksyb.Common.Data
{
    public class IsNullOrWhiteSpace_Handler : IMethodHandler
    {
        public static readonly string MethodName = "IsNullOrWhiteSpace";
        private static readonly MethodInfo StringIsNullOrWhiteSpace = typeof(string).GetMethod(MethodName, new Type[] { typeof(string) });

        public bool CanProcess(DbMethodCallExpression exp)
        {
            if (exp.Method != StringIsNullOrWhiteSpace)
                return false;

            return true;
        }

        public void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            DbMethodCallExpression trim = DbExpression.MethodCall(exp.Arguments.First(), PublicConstants.MethodInfo_String_Trim, new List<DbExpression>() { });

            var equalNullExpression = DbExpression.Equal(trim, DbExpression.Constant(null, PublicConstants.TypeOfString));
            var equalEmptyExpression = DbExpression.Equal(trim, DbExpression.Constant(string.Empty));

            var orExpression = DbExpression.Or(equalNullExpression, equalEmptyExpression);

            var whenThenPair = new DbCaseWhenExpression.WhenThenExpressionPair(orExpression, DbConstantExpression.One);

            var whenThenExps = new List<DbCaseWhenExpression.WhenThenExpressionPair>(1)
            {
                whenThenPair
            };

            DbCaseWhenExpression caseWhenExpression = DbExpression.CaseWhen(whenThenExps, DbConstantExpression.Zero, PublicConstants.TypeOfBoolean);

            var eqExp = DbExpression.Equal(caseWhenExpression, DbConstantExpression.One);
            eqExp.Accept(generator);
        }
    }
}