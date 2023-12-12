using Chloe.Infrastructure;
using Chloe.MySql;
using Chloe.RDBMS;
using MySqlConnector;
using System.Data;

namespace Gksyb.Common.Data
{
    public class MySqlConnectionFactory : IDbConnectionFactory
    {
        static MySqlConnectionFactory()//初始化
        {
            var methodHandlerDic = new Dictionary<string, IMethodHandler>()
            {
                {IsNullOrWhiteSpace_Handler.MethodName, new IsNullOrWhiteSpace_Handler() },
                {Compare_Handler.MethodName, new Compare_Handler()},
                {Contains_Handler.MethodName, new ContainsConcat_Handler()},
                {SumString_Handler.MethodName, new SumString_Handler("GROUP_CONCAT",null)}
            };
            foreach (var item in methodHandlerDic)
            {
                MySqlContext.SetMethodHandler(item.Key, item.Value);
            }
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