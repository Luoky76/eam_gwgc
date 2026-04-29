using Chloe.DbExpressions;
using Chloe.RDBMS;

namespace Gksyb.Common.Data
{
    public class MathFloor_Handler : IMethodHandler
    {
        public const string MethodName = nameof(Math.Floor);

        public bool CanProcess(DbMethodCallExpression exp) => exp.Method.DeclaringType == typeof(Math) && (exp.Method.Name == MethodName);

        public virtual void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            generator.SqlBuilder.Append("FLOOR(");
            exp.Arguments[0].Accept(generator);
            generator.SqlBuilder.Append(")");
        }
    }

    public class MathFloorRound_Handler : MathFloor_Handler
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            generator.SqlBuilder.Append("ROUND((");
            exp.Arguments[0].Accept(generator);
            generator.SqlBuilder.Append(" - 0.499999),0)");
        }
    }
}