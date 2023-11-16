using Chloe.Infrastructure;
using Chloe.SQLite;
using Microsoft.Data.Sqlite;
using System.Data;

namespace Gksyb.Common.Data
{
    public class SQLiteConnectionFactory : IDbConnectionFactory
    {
        static SQLiteConnectionFactory()//初始化
        {
            SQLiteContext.SetMethodHandler(IsNullOrWhiteSpace_Handler.MethodName, new IsNullOrWhiteSpace_Handler());
            SQLiteContext.SetMethodHandler(Compare_Handler.MethodName, new Compare_Handler());
        }

        private readonly string _connString = null;

        public SQLiteConnectionFactory(string connString)
        {
            //如果是~则表示当前目录
            if (connString.Contains("~/") || connString.Contains("~\\"))
            {
                var rootPath = AppDomain.CurrentDomain.BaseDirectory;
                var index = rootPath.IndexOf("\\bin\\");
                if (index > 0)
                {
                    rootPath = Path.Combine(rootPath[..index], "");
                }
                connString = connString.Replace("/", "\\").Replace("~\\", rootPath.TrimEnd('\\') + "\\");
            }
            this._connString = connString;
        }

        public IDbConnection CreateConnection()
        {
            IDbConnection conn = new SqliteConnection(this._connString);
            return conn;
        }
    }
}