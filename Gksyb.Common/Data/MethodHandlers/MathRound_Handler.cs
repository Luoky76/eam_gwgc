using Chloe.DbExpressions;
using Chloe.RDBMS;

namespace Gksyb.Common.Data
{
    public class MathRound_Handler : IMethodHandler
    {
        public const string MethodName = nameof(Math.Round);

        public bool CanProcess(DbMethodCallExpression exp) => exp.Method.DeclaringType == typeof(Math) && (exp.Method.Name == MethodName);

        public virtual void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            generator.SqlBuilder.Append("ROUND(");
            exp.Arguments[0].Accept(generator);
            if (exp.Arguments.Count > 1)
            {
                generator.SqlBuilder.Append(",");
                exp.Arguments[1].Accept(generator);
            }
            generator.SqlBuilder.Append(")");
        }
    }
}