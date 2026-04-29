using Chloe.Infrastructure;
using Chloe.RDBMS;
using Chloe.SQLite;
using Microsoft.Data.Sqlite;
using System.Data;

namespace Gksyb.Common.Data
{
    public class SQLiteConnectionFactory : IDbConnectionFactory
    {
        static SQLiteConnectionFactory()//初始化
        {
            var methodHandlerDic = new Dictionary<string, IMethodHandler>()
            {
                {IsNullOrWhiteSpace_Handler.MethodName, new IsNullOrWhiteSpace_Handler() },
                {Compare_Handler.MethodName, new Compare_Handler()},
                {Contains_Handler.MethodName, new Contains_Handler()},
                {SumString_Handler.MethodName, new SumString_Handler("GROUP_CONCAT",null)},
                {MathAbs_Handler.MethodName, new MathAbs_Handler()},
                {MathRound_Handler.MethodName, new MathRound_Handler()},
                {MathFloor_Handler.MethodName, new MathFloorRound_Handler()},
                {MathCeiling_Handler.MethodName, new MathCeilingRound_Handler()}
            };
            foreach (var item in methodHandlerDic)
            {
                SQLiteContext.SetMethodHandler(item.Key, item.Value);
            }
        }

        private readonly string _connString = null;

        public SQLiteConnectionFactory(string connString)
        {
            this._connString = MapPath(connString);
        }

        public IDbConnection CreateConnection()
        {
            IDbConnection conn = new SqliteConnection(this._connString);
            return conn;
        }

        private static readonly string[] _splits = new string[] { "~\\", "~/" };

        /// <summary>
        /// 获取真实连接字符串
        /// </summary>
        public static string MapPath(string connString)
        {
            foreach (var split in _splits)
            {
                if (!connString.Contains(split)) continue;
                var path = AppDomain.CurrentDomain.BaseDirectory;
                var index = path.IndexOf($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}");
                if (index > 0)
                {
                    path = $"{Path.Combine(path[..index], "")}{Path.DirectorySeparatorChar}";
                }
                connString = connString.Replace(split, path);
                break;
            }
            connString = connString.Replace('\\', Path.DirectorySeparatorChar);
            return connString;
        }
    }
}