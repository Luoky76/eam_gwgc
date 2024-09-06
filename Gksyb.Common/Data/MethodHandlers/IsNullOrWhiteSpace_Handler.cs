using Chloe;
using Chloe.DbExpressions;
using Chloe.RDBMS;
using System.Reflection;

namespace Gksyb.Common.Data
{
    public class IsNullOrWhiteSpace_Handler : IMethodHandler
    {
        public const string MethodName = nameof(string.IsNullOrWhiteSpace);
        private static readonly MethodInfo method = typeof(string).GetMethod(MethodName, new Type[] { typeof(string) });

        public bool CanProcess(DbMethodCallExpression exp) => exp.Method == method;

        public void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            var trim = new DbMethodCallExpression(exp.Arguments.First(), PublicConstants.MethodInfo_String_Trim, new List<DbExpression>() { });
            DbExpression expression = new DbEqualExpression(trim, DbConstantExpression.StringNull);
            if (!generator.Options.TreatEmptyStringAsNull)
            {
                var equalEmptyExpression = new DbEqualExpression(trim, DbConstantExpression.StringEmpty);
                expression = new DbOrExpression(expression, equalEmptyExpression);
            }
            expression.Accept(generator);
        }
    }
}