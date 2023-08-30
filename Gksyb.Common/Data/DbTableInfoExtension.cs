#pragma warning disable IDE0051 // 删除未使用的私有成员
using Chloe;
using System.Reflection;

namespace Gksyb.Common.Data
{
    public static class DbTableInfoExtension
    {
        public const string TableType = "table";
        public const string ViewType = "view";

        /// <summary>
        /// 获取表、视图信息
        /// </summary>
        public static async Task<List<DbTableInfo>> GetTables(this IDbContext source, string type = null)
        {
            var method = $"Get{source.GetDbType()}Tables";
            MessageException.ThrowIf(!_methodInfos.ContainsKey(method), $"不支持{method}");
            var list = await (_methodInfos[method].Invoke(null, new object[] { source, type }) as Task<List<DbTableInfo>>);
            return list.OrderBy(c => c.Name).ToList();
        }

        /// <summary>
        /// 获取视图信息
        /// </summary>
        public static async Task<List<DbTableInfo>> GetViews(this IDbContext source)
        {
            var method = $"Get{source.GetDbType()}Tables";
            MessageException.ThrowIf(!_methodInfos.ContainsKey(method), $"不支持{method}");
            var list = await (_methodInfos[method].Invoke(null, new object[] { source, ViewType }) as Task<List<DbTableInfo>>);
            return list.OrderBy(c => c.Name).ToList();
        }

        /// <summary>
        /// 获取oracle表信息
        /// </summary>
        private static async Task<List<DbTableInfo>> GetOracleTables(IDbContext source, string type)
        {
            if (type == ViewType) return await GetOracleViews(source);
            var sql = $@"SELECT a.table_name ""Name"", b.comments ""Comment"", a.owner ""Schema"",'{TableType}' ""Type""
                          FROM all_tables a
                          LEFT JOIN all_tab_comments b
                            ON a.table_name = b.table_name
                           AND a.owner = b.owner
                         WHERE (a.owner IN (SELECT username FROM user_users) OR EXISTS
                                (SELECT 1
                                   FROM user_tab_privs t
                                  WHERE a.table_name = t.table_name
                                    AND a.owner = t.owner
                                    AND t.privilege = 'SELECT'))";
            var tables = await source.SqlQueryAsync<DbTableInfo>(sql);
            if (string.IsNullOrWhiteSpace(type))
            {
                tables.AddRange(await GetOracleViews(source));
            }
            return tables;
        }

        /// <summary>
        /// 获取oracle视图
        /// </summary>
        private static async Task<List<DbTableInfo>> GetOracleViews(IDbContext source)
        {
            var sql = $@"SELECT a.view_name ""Name"", b.comments ""Comment"", a.owner ""Schema"",'{ViewType}' ""Type""
                          FROM all_views a
                          LEFT JOIN all_tab_comments b
                            ON a.view_name = b.table_name
                           AND a.owner = b.owner
                         WHERE (a.owner IN (SELECT username FROM user_users) OR EXISTS
                                (SELECT 1
                                   FROM user_tab_privs t
                                  WHERE a.view_name = t.table_name
                                    AND a.owner = t.owner
                                    AND t.privilege = 'SELECT'))";
            return await source.SqlQueryAsync<DbTableInfo>(sql);
        }

        /// <summary>
        /// 获取mysql表信息
        /// </summary>
        private static async Task<List<DbTableInfo>> GetMysqlTables(IDbContext source, string type)
        {
            var where = type switch
            {
                ViewType => "AND TABLE_TYPE = 'VIEW'",
                TableType => "AND TABLE_TYPE = 'BASE TABLE'",
                _ => "AND TABLE_TYPE in ('BASE TABLE','VIEW')",
            };
            var sql = $@"SELECT TABLE_NAME ""Name"", TABLE_COMMENT ""Comment"",TABLE_SCHEMA ""Schema"",(CASE WHEN TABLE_TYPE = 'VIEW' THEN '{ViewType}' ELSE '{TableType}' END) ""Type""
                          FROM information_schema.TABLES
                         WHERE TABLE_SCHEMA = (SELECT DATABASE()) {where}";
            return await source.SqlQueryAsync<DbTableInfo>(sql);
        }

        /// <summary>
        /// 获取mysql表信息
        /// </summary>
        private static async Task<List<DbTableInfo>> GetSqliteTables(IDbContext source, string type)
        {
            var where = string.IsNullOrEmpty(type) ? $"AND type in ('{TableType}','{ViewType}')" : $"AND type = '{type}'";
            var sql = $@"SELECT name,type
                          FROM sqlite_master
                         WHERE name <> 'sqlite_sequence' {where}";
            return await source.SqlQueryAsync<DbTableInfo>(sql);
        }

        /// <summary>
        /// 获取mysql表信息
        /// </summary>
        private static async Task<List<DbTableInfo>> GetPgsqlTables(IDbContext source, string type)
        {
            if (type == ViewType) return await GetPgsqlViews(source);
            var sql = $@"SELECT CAST(relname AS VARCHAR) Name,
                           CAST(obj_description(c.oid, 'pg_class') AS VARCHAR) Comment,'{TableType}' Type
                      FROM pg_class c
                     INNER JOIN pg_namespace n
                        ON n.oid = c.relnamespace
                       AND nspname = 'public'
                     INNER JOIN pg_tables z
                        ON z.tablename = c.relname
                     WHERE relkind IN ('p', 'r')
                       AND relname NOT LIKE 'pg_%'
                       AND relname NOT LIKE 'sql_%'
                       AND schemaname = 'public'";
            var tables = await source.SqlQueryAsync<DbTableInfo>(sql);
            if (string.IsNullOrWhiteSpace(type))
            {
                tables.AddRange(await GetPgsqlViews(source));
            }
            return tables;
        }

        /// <summary>
        /// 获取mysql表信息
        /// </summary>
        private static async Task<List<DbTableInfo>> GetPgsqlViews(IDbContext source)
        {
            var sql = $@"SELECT CAST(relname AS VARCHAR) Name,
                               CAST(Description AS VARCHAR) Comment,'{ViewType}' Type
                          FROM pg_description
                          JOIN pg_class
                            ON pg_description.objoid = pg_class.oid
                         WHERE objsubid = 0
                           AND exists(select 1 from pg_views t where relname = t.viewname and t.schemaname = 'public')";
            return await source.SqlQueryAsync<DbTableInfo>(sql);
        }

        /// <summary>
        /// 获取mysql表信息
        /// </summary>
        private static async Task<List<DbTableInfo>> GetSqlServerTables(IDbContext source, string type)
        {
            var where = type switch
            {
                ViewType => "AND a.type = 'V'",
                TableType => "AND a.type = 'U'",
                _ => "AND a.type in ('U','V')",
            };
            var sql = $@"SELECT a.name Name, b.value Comment,(CASE WHEN a.type = 'V' THEN '{ViewType}' ELSE '{TableType}' END) TYPE
                         FROM sys.objects a LEFT JOIN sys.extended_properties b ON a.object_id = b.major_id AND b.minor_id = 0 
                         WHERE 1 = 1 {where}";
            return await source.SqlQueryAsync<DbTableInfo>(sql);
        }

        /// <summary>
        /// 获取达梦表信息
        /// </summary>
        private static async Task<List<DbTableInfo>> GetDamengTables(IDbContext source, string type)
        {
            return await GetOracleTables(source, type);
        }

        private static readonly Dictionary<string, MethodInfo> _methodInfos = null;

        /// <summary>
        /// 初始化
        /// </summary>
        static DbTableInfoExtension()
        {
            _methodInfos = typeof(DbTableInfoExtension).GetDicMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.IgnoreCase);
        }
    }
}
#pragma warning restore IDE0051 // 删除未使用的私有成员