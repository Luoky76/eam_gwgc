using Chloe;
using Chloe.Dameng;
using Chloe.KingbaseES;
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
                "oracle" => new OracleContext(new OracleOptions { DbConnectionFactory = new OracleConnectionFactory(connString), InsertStrategy = InsertStrategy.IgnoreNull | InsertStrategy.IgnoreEmptyString }),
                "mysql" => new MySqlContext(new MySqlOptions { DbConnectionFactory = new MySqlConnectionFactory(connString), InsertStrategy = InsertStrategy.IgnoreNull | InsertStrategy.IgnoreEmptyString }),
                "mssql" => new MsSqlContext(new MsSqlOptions() { DbConnectionFactory = new MsSqlConnectionFactory(connString), InsertStrategy = InsertStrategy.IgnoreNull | InsertStrategy.IgnoreEmptyString, PagingMode = PagingMode.OFFSET_FETCH }),
                "sqlserver" => new MsSqlContext(new MsSqlOptions { DbConnectionFactory = new MsSqlConnectionFactory(connString), InsertStrategy = InsertStrategy.IgnoreNull | InsertStrategy.IgnoreEmptyString }),
                "sqlite" => new SQLiteContext(new SQLiteOptions { DbConnectionFactory = new SQLiteConnectionFactory(connString), InsertStrategy = InsertStrategy.IgnoreNull | InsertStrategy.IgnoreEmptyString }),
                "pgsql" => new PostgreSQLContext(new PostgreSQLOptions { DbConnectionFactory = new PostgreSQLConnectionFactory(connString), InsertStrategy = InsertStrategy.IgnoreNull | InsertStrategy.IgnoreEmptyString }),
                "dameng" => new DamengContext(new DamengOptions { DbConnectionFactory = new DamengConnectionFactory(connString), InsertStrategy = InsertStrategy.IgnoreNull | InsertStrategy.IgnoreEmptyString }),
                "kingbase" => new KingbaseESContext(new KingbaseESOptions { DbConnectionFactory = new KingbaseESConnectionFactory(connString), InsertStrategy = InsertStrategy.IgnoreNull | InsertStrategy.IgnoreEmptyString }),
                _ => new MsSqlContext(new MsSqlOptions() { DbConnectionFactory = new MsSqlConnectionFactory(connString), InsertStrategy = InsertStrategy.IgnoreNull | InsertStrategy.IgnoreEmptyString, PagingMode = PagingMode.OFFSET_FETCH })
            };
            return dbContext;
        }
    }
}