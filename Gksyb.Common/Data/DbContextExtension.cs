using Chloe.Core;
using Chloe.DbExpressions;
using Chloe.Descriptors;
using Chloe.Entity;
using Chloe.Extension;
using Chloe.Infrastructure;
using Gksyb.Common;
using Gksyb.Common.Data;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe
{
    /// <summary>
    /// DbContext扩展
    /// </summary>
    public static class DbContextExtension
    {
        /// <summary>
        /// 插入或删除
        /// </summary>
        public static T InsertOrUpdate<T>(this IDbContext source, T entity, Expression<Func<T, bool>> condition = null)
        {
            return source.InsertOrUpdateAsync<T>(entity, condition).Result();
        }

        /// <summary>
        /// 插入或删除
        /// </summary>
        public static async Task<T> InsertOrUpdateAsync<T>(this IDbContext source, T entity, Expression<Func<T, bool>> condition = null)
        {
            var fakes = source.TryGetTrackedEntityState(entity).Fakes;
            var oldFakes = new Dictionary<MemberInfo, object>(fakes);
            var task = condition == null ? source.UpdateAsync(entity) : source.UpdateAsync(entity, condition);
            var row = await task;
            if (row < 1)
            {
                var keys = fakes.Keys;
                foreach (var key in keys)
                {
                    fakes[key] = oldFakes[key];
                }
                return await source.InsertAsync(entity);
            }
            return entity;
        }

        /// <summary>
        /// 获取临时变量
        /// </summary>
        /// <returns></returns>
        public static T GetItem<T>(this IDbContext source, string key)
        {
            if (source.Items.TryGetValue(key, out object value)) return (T)value;
            return default;
        }

        /// <summary>
        /// 设置临时变量
        /// </summary>
        public static void SetItem<T>(this IDbContext source, string key, T value)
        {
            if (source.Items.ContainsKey(key))
            {
                source.Items[key] = value;
                return;
            }
            source.Items.Add(key, value);
        }

        /// <summary>
        /// 开启事务 已开启的事务不再重新开启
        /// </summary>
        /// <returns></returns>
        public static async Task UseTransactionAsync(this IDbContext source, Func<Task> action)
        {
            await source.UseTransaction(action);
        }

        /// <summary>
        /// 复制连接
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IDbContext Clone(this IDbContext source)
        {
            if (source is DbContext dbContext)
            {
                using IDbConnection dbConnection = dbContext.DatabaseProvider.CreateConnection();
                var clone = DbContextFactory.CreateContext(dbContext.DatabaseProvider.DatabaseType, dbConnection.ConnectionString);
                source.SetItem(GuidHelper.NewShortId(), clone);
                return clone;
            }
            return null;
        }

        /// <summary>
        /// 清除所有更新列
        /// </summary>
        public static void ClearModifyFields<T>(this IDbContext source, T entity)
        {
            source.TryGetTrackedEntityState(entity)?.Refresh();
        }

        /// <summary>
        /// 设置更新列
        /// </summary>
        public static void SetChangedFields<T>(this IDbContext source, T entity, Expression<Func<T, object>> fields)
        {
            source.SetChangedFields(entity, FieldsResolver.Resolve(fields));
        }

        /// <summary>
        /// 设置更新列
        /// </summary>
        public static void SetChangedFields<T>(this IDbContext source, T entity, List<string> fields)
        {
            source.SetChangedFields(entity, fields, default);
        }

        /// <summary>
        /// 设置更新列
        /// </summary>
        public static void SetChangedFields<T>(this IDbContext source, T entity, Expression<Func<T, object>> fields, T orgin)
        {
            source.SetChangedFields(entity, FieldsResolver.Resolve(fields), orgin);
        }

        /// <summary>
        /// 设置更新列
        /// </summary>
        public static void SetChangedFields<T>(this IDbContext source, T entity, List<string> fields, T orgin)
        {
            IEntityState entityState = source.TryGetTrackedEntityState(entity);
            var fakes = entityState.Fakes;
            var keys = fakes.Keys;
            var hasOrgin = orgin != null;
            foreach (var field in fields)
            {
                if (hasOrgin)
                {
                    var propertyDescriptor = entityState.TypeDescriptor.PrimitivePropertyDescriptors.FirstOrDefault(a => a.Column.Name == field);
                    if (propertyDescriptor != null)
                    {
                        fakes[propertyDescriptor.Definition.Property] = propertyDescriptor.GetValue(orgin);
                        continue;
                    }
                }
                var key = keys.FirstOrDefault(a => a.Name == field);
                if (key != null) fakes.Remove(key);
            }
        }

        /// <summary>
        /// 获取主键值
        /// </summary>
        /// <returns></returns>
        public static Dictionary<string, object> GetPrimaryValues<TEntity>(this IDbContext source, TEntity entity, TypeDescriptor typeDescriptor = null)
        {
            var keyValueMap = new Dictionary<string, object>();
            if (source == null) return keyValueMap;
            typeDescriptor ??= EntityTypeContainer.GetDescriptor(typeof(TEntity));
            foreach (PrimitivePropertyDescriptor propertyDescriptor in typeDescriptor.PrimaryKeys)
            {
                var key = propertyDescriptor.GetDescription();
                keyValueMap.Add(key, propertyDescriptor.GetValue(entity));
            }
            return keyValueMap;
        }

        /// <summary>
        /// 获取数据库时间 默认会进行缓存
        /// </summary>
        /// <param name="source"></param>
        /// <param name="isCache">是否从缓存获取</param>
        /// <returns></returns>
        public static async Task<DateTime?> GetSysdate(this IDbContext source, bool isCache = true)
        {
            DateTime? sysdate = default;
            var key = "Sysdate";
            if (isCache && source.Items.TryGetValue(key, out object value))
            {
                sysdate = value as DateTime?;
            }
            if (!sysdate.HasValue)
            {
                await source.NotSqlLog(async () =>
                {
                    sysdate = (await source.Session.ExecuteScalarAsync("select sysdate from dual")).CastTo<DateTime?>();
                });
                if (isCache && sysdate.HasValue)
                {
                    source.SetItem(key, sysdate);
                }
            }
            return sysdate;
        }

        /// <summary>
        /// 获取变量前缀
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetParamPrefix(this IDbContext source)
        {
            if (source is Oracle.OracleContext)
            {
                return ":";
            }
            else if (source is MySql.MySqlContext)
            {
                return "?";
            }
            return "@";
        }

        /// <summary>
        /// 获取数据库类型
        /// </summary>
        /// <returns></returns>
        public static string GetDbType(this IDbContext source)
        {
            return source switch
            {
                Oracle.OracleContext => "oracle",
                MySql.MySqlContext => "mysql",
                SQLite.SQLiteContext => "sqlite",
                PostgreSQL.PostgreSQLContext => "pgsql",
                SqlServer.MsSqlContext => "sqlserver",
                _ => "oracle",
            };
        }

        /// <summary>
        /// 获取分页语句
        /// </summary>
        /// <returns></returns>
        public static string GetPageSql(this IDbContext source)
        {
            return source switch
            {
                Oracle.OracleContext => "SELECT T.* FROM (SELECT A.*,ROWNUM AS RN FROM ({View} {Order}) A WHERE ROWNUM <= ({N} + {M})) T WHERE T.RN > {N}",
                MySql.MySqlContext => "{View} {Order} LIMIT {N},{M}",
                SqlServer.MsSqlContext => "SELECT TOP {M} T.* FROM (SELECT *,ROW_NUMBER() OVER({Order}) AS RN FROM ({View}) A) T WHERE T.RN > {N} ORDER BY T.RN ASC",
                _ => "{View} {Order} LIMIT {M} OFFSET {N}",
            };
        }

        /// <summary>
        /// 获取表达式翻译器
        /// </summary>
        /// <returns></returns>
        public static IDbExpressionTranslator GetDbExpressionTranslator(this IDbContext source)
        {
            return (source as DbContext).DatabaseProvider.CreateDbExpressionTranslator();
        }

        /// <summary>
        /// 翻译表达式
        /// </summary>
        /// <returns></returns>
        public static DbCommandInfo Translate(this IDbContext source, DbExpression e)
        {
            var translator = source.GetDbExpressionTranslator();
            var dbCommandInfo = translator.Translate(e);
            return dbCommandInfo;
        }

        /// <summary>
        /// 翻译成sql
        /// </summary>
        /// <returns></returns>
        public static string ToSql(this DbCommandInfo source)
        {
            return source.ToSql(null);
        }

        /// <summary>
        /// 翻译成sql
        /// </summary>
        /// <returns></returns>
        public static string ToSql(this DbCommandInfo source, string key)
        {
            var sql = source.CommandText;
            if (source.Parameters == null || source.Parameters.Count < 1) return sql;
            if (!string.IsNullOrWhiteSpace(key) && source.Parameters.Count == 1 && source.CommandText.Contains(key, StringComparison.OrdinalIgnoreCase))
                return source.Parameters[0].Value?.ToString();
            foreach (var parameter in source.Parameters)
            {
                sql = sql.Replace(parameter.Name, $"'{parameter.Value}'");
            }
            return sql;
        }

        /// <summary>
        /// 获取列描述
        /// </summary>
        /// <returns></returns>
        public static string GetDescription(this PrimitivePropertyDescriptor source)
        {
            var key = source.Column.Name;
            foreach (var annotation in source.Definition.Annotations)
            {
                if (annotation is System.ComponentModel.DescriptionAttribute attribute)
                {
                    key = attribute?.Description;
                    break;
                }
            }
            return key;
        }

        /// <summary>
        /// 获取列描述
        /// </summary>
        /// <returns></returns>
        public static string GetDescription(this TypeDefinition source)
        {
            var key = source.Table.Name;
            foreach (var annotation in source.Annotations)
            {
                if (annotation is System.ComponentModel.DescriptionAttribute attribute)
                {
                    key = attribute?.Description;
                    break;
                }
            }
            return key;
        }

        /// <summary>
        /// 获取表达式的计算值
        /// </summary>
        /// <returns></returns>
        public static object Evaluate(this IDbContext source, DbExpression expression)
        {
            if (expression is DbParameterExpression dbParameterExpression) return dbParameterExpression.Value;
            var dbCommandInfo = source.Translate(expression);
            if (dbCommandInfo.Parameters?.Count == 1 && dbCommandInfo.CommandText == dbCommandInfo.Parameters[0].Name) return dbCommandInfo.Parameters[0].Value;
            return dbCommandInfo.ToSql();
        }

        #region sql日志

        /// <summary>
        /// 不写入数据库日志
        /// </summary>
        /// <returns></returns>
        public static async Task NotSqlLog(this IDbContext source, Func<Task> func)
        {
            var isSqlLog = !source.IsDisableSqlLog();
            try
            {
                if (isSqlLog) source.DisableSqlLog();
                await func();
            }
            finally
            {
                if (isSqlLog) source.EnableSqlLog();
            }
        }

        /// <summary>
        /// 关闭数据库日志标识
        /// </summary>
        private static readonly string _sqlLogKey = "NotSqlLog";

        /// <summary>
        /// 是否关闭数据库日志
        /// </summary>
        /// <returns></returns>
        public static bool IsDisableSqlLog(this IDbContext source)
        {
            return source.GetItem<bool>(_sqlLogKey);
        }

        /// <summary>
        /// 关闭数据库日志
        /// </summary>
        /// <returns></returns>
        public static void DisableSqlLog(this IDbContext source)
        {
            source.SetItem(_sqlLogKey, true);
        }

        /// <summary>
        /// 启用数据库日志
        /// </summary>
        /// <returns></returns>
        public static void EnableSqlLog(this IDbContext source)
        {
            source.Items.Remove(_sqlLogKey);
        }

        #endregion sql日志
    }
}