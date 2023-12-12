using Chloe.DbExpressions;
using Chloe.RDBMS;
using System.Reflection;

namespace Gksyb.Common.Data
{
    public class Contains_Handler : IMethodHandler
    {
        public static readonly string MethodName = nameof(string.Contains);
        private static readonly MethodInfo method = typeof(string).GetMethod(MethodName, new Type[] { typeof(char) });

        private string _concat;

        public Contains_Handler(string concat = "||")
        {
            _concat = concat;
        }

        public bool CanProcess(DbMethodCallExpression exp) => exp.Method == method;

        public virtual void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            exp.Object.Accept(generator);
            generator.SqlBuilder.Append($" like '%' {_concat} ");
            exp.Arguments.First().Accept(generator);
            generator.SqlBuilder.Append($" {_concat} '%'");
        }
    }

    public class ContainsConcat_Handler : Contains_Handler
    {
        public override void Process(DbMethodCallExpression exp, SqlGeneratorBase generator)
        {
            exp.Object.Accept(generator);
            generator.SqlBuilder.Append(" like CONCAT('%',");
            exp.Arguments.First().Accept(generator);
            generator.SqlBuilder.Append(",'%')");
        }
    }
}