using Chloe;
using Chloe.DbExpressions;
using Chloe.RDBMS;

namespace Gksyb.Common.Data
{
    public class MathAbs_Handler : IMethodHandler
    {
        public const string MethodName = nameof(Math.Abs);

        public bool CanProcess(DbMethodCallExpression exp) => exp.Method.DeclaringType == PublicConstants.TypeOfMath && (exp.Method.Name == MethodName);

        public virtual void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            generator.SqlBuilder.Append("ABS(");
            exp.Arguments[0].Accept(generator);
            generator.SqlBuilder.Append(")");
        }
    }
}