using Chloe;
using Chloe.Dameng;
using Chloe.MySql;
using Chloe.Oracle;
using Chloe.PostgreSQL;
using Chloe.SQLite;
using Chloe.SqlServer;

namespace Gksyb.Common.Data
{
    /// <summary>
    /// DbContext工厂
    /// </summary>
    public class DbContextFactory
    {
        public static string ConnectionString { get; private set; }
        public static string DbType { get; private set; }

        /// <summary>
        /// 设置默认连接的数据库
        /// </summary>
        public static void SetDefault(string dbType, string connString)
        {
            DbType = dbType;
            ConnectionString = connString;
        }

        /// <summary>
        /// 创建IDbContext
        /// </summary>
        /// <returns></returns>
        public static IDbContext CreateContext()
        {
            return CreateContext(DbType, ConnectionString);
        }

        /// <summary>
        /// 创建IDbContext
        /// </summary>
        /// <param name="dbType">数据库类型</param>
        /// <param name="connString">连接字符串</param>
        /// <returns></returns>
        public static IDbContext CreateContext(string dbType, string connString)
        {
            dbType = (dbType ?? "").Trim().ToLower();
            IDbContext dbContext = dbType switch
            {
                "oracle" => new OracleContext(new OracleConnectionFactory(connString)),
                "mysql" => new MySqlContext(new MySqlConnectionFactory(connString)),
                "mssql" => new MsSqlContext(new MsSqlConnectionFactory(connString), PagingMode.OFFSET_FETCH),
                "sqlserver" => new MsSqlContext(new MsSqlConnectionFactory(connString)),
                "sqlite" => new SQLiteContext(new SQLiteConnectionFactory(connString)),
                "pgsql" => new PostgreSQLContext(new PostgreSQLConnectionFactory(connString)),
                "dameng" => new DamengContext(new DamengConnectionFactory(connString)),
                _ => new MsSqlContext(new MsSqlConnectionFactory(connString), PagingMode.OFFSET_FETCH),
            };
            return dbContext;
        }
    }
}