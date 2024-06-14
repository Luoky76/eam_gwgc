using Chloe.Infrastructure;
using Chloe.PostgreSQL;
using Chloe.RDBMS;
using Npgsql;
using System.Data;

namespace Gksyb.Common.Data
{
    public class PostgreSQLConnectionFactory : IDbConnectionFactory
    {
        static PostgreSQLConnectionFactory()//初始化
        {
            var methodHandlerDic = new Dictionary<string, IMethodHandler>()
            {
                {IsNullOrWhiteSpace_Handler.MethodName, new IsNullOrWhiteSpace_Handler() },
                {Compare_Handler.MethodName, new Compare_Handler()},
                {Contains_Handler.MethodName, new Contains_Handler()},
                {SumString_Handler.MethodName, new SumString_Handler()},
                {MathAbs_Handler.MethodName, new MathAbs_Handler()}
            };
            foreach (var item in methodHandlerDic)
            {
                PostgreSQLContext.SetMethodHandler(item.Key, item.Value);
            }
        }

        private readonly string _connString = null;

        public PostgreSQLConnectionFactory(string connString)
        {
            this._connString = connString;
        }

        public IDbConnection CreateConnection()
        {
            IDbConnection conn = new NpgsqlConnection(this._connString);
            return conn;
        }
    }
}