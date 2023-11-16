using Chloe.Infrastructure;
using Chloe.MySql;
using MySqlConnector;
using System.Data;

namespace Gksyb.Common.Data
{
    public class MySqlConnectionFactory : IDbConnectionFactory
    {
        static MySqlConnectionFactory()//初始化
        {
            MySqlContext.SetMethodHandler(IsNullOrWhiteSpace_Handler.MethodName, new IsNullOrWhiteSpace_Handler());
            MySqlContext.SetMethodHandler(Compare_Handler.MethodName, new Compare_Handler());
        }

        private readonly string _connString = null;

        public MySqlConnectionFactory(string connString)
        {
            this._connString = connString;
        }

        public IDbConnection CreateConnection()
        {
            IDbConnection conn = new MySqlConnection(this._connString);
            return conn;
        }
    }
}