using Chloe.DbExpressions;
using Chloe.RDBMS;

namespace Gksyb.Common.Data
{
    public class MathCeiling_Handler : IMethodHandler
    {
        private readonly string _name;
        public const string MethodName = nameof(Math.Ceiling);

        public MathCeiling_Handler(string name = "CEIL")
        {
            _name = name;
        }

        public bool CanProcess(DbMethodCallExpression exp) => exp.Method.DeclaringType == typeof(Math) && (exp.Method.Name == MethodName);

        public virtual void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            generator.SqlBuilder.Append($"{_name}(");
            exp.Arguments[0].Accept(generator);
            generator.SqlBuilder.Append(")");
        }
    }



    public class MathCeilingRound_Handler : MathCeiling_Handler
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            generator.SqlBuilder.Append("ROUND((");
            exp.Arguments[0].Accept(generator);
            generator.SqlBuilder.Append(" + 0.499999),0)");
        }
    }
}