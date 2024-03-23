using Chloe;
using Chloe.DbExpressions;
using Chloe.RDBMS;

namespace Gksyb.Common.Data
{
    public class SumString_Handler : IMethodHandler
    {
        public const string MethodName = "Sum";

        private readonly string _fn;
        private readonly string _delimiter;
        private readonly string _dis;

        public SumString_Handler(string fn = "STRING_AGG", string delimiter = ",", string dis = "DISTINCT")
        {
            _fn = fn;
            _delimiter = delimiter;
            _dis = dis;
        }

        public bool CanProcess(DbMethodCallExpression exp) => exp.Method.DeclaringType == PublicConstants.TypeOfSql && (exp.Method.ReturnType == typeof(string));

        public void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            generator.SqlBuilder.Append($"{_fn}({_dis}(");
            exp.Arguments.First().Accept(generator);
            generator.SqlBuilder.Append(")");
            if (!string.IsNullOrWhiteSpace(_delimiter))
            {
                generator.SqlBuilder.Append($",'{_delimiter}'");
            }
            generator.SqlBuilder.Append(")");
        }
    }
}