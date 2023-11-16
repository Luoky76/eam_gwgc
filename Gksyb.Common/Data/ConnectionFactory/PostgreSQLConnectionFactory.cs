using Chloe.Infrastructure;
using Chloe.PostgreSQL;
using Npgsql;
using System.Data;

namespace Gksyb.Common.Data
{
    public class PostgreSQLConnectionFactory : IDbConnectionFactory
    {
        static PostgreSQLConnectionFactory()//初始化
        {
            PostgreSQLContext.SetMethodHandler(IsNullOrWhiteSpace_Handler.MethodName, new IsNullOrWhiteSpace_Handler());
            PostgreSQLContext.SetMethodHandler(Compare_Handler.MethodName, new Compare_Handler());
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