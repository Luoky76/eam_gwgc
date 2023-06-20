using Chloe.Annotations;
using Chloe.Descriptors;
using Chloe.Entity;
using Chloe.Infrastructure;
using Gksyb.Common.Data;
using Gksyb.Common.Static;
using Gksyb.Core.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        /// 根据配置表获取数据库连接
        /// </summary>
        /// <param name="source">数据库上下文</param>
        /// <param name="linkName">配置名称</param>
        /// <param name="isCache">是否从缓存获取</param>
        /// <returns></returns>
        public static async Task<IDbContext> GetDbContext(this IDbContext source, string linkName, bool isCache = true)
        {
            if (string.IsNullOrWhiteSpace(linkName)) return source;
            IDbContext dbContext = null;
            if (isCache)
            {
                dbContext = source.GetItem<IDbContext>(linkName);
                if (dbContext != null && !dbContext.IsDispose) return dbContext;
            }
            TDBLINK tDBLinkEntity = null;
            await source.NotSqlLog(async () =>
            {
                tDBLinkEntity = await source.Query<TDBLINK>().Where(c => c.LINKNAME == linkName).FirstAsync();
            });
            var dbType = (tDBLinkEntity.LINKTYPE ?? "oracle").ToLower();
            switch (dbType)//历史遗留
            {
                case "odp":
                    dbType = "oracle";
                    break;

                case "sqlserver9":
                    dbType = "sqlserver";
                    break;
            }
            dbContext = DbContextFactory.CreateContext(dbType, tDBLinkEntity.CONNSTR);
            if (!isCache) linkName = GuidHelper.NewShortId();
            source.SetItem(linkName, dbContext);
            if (tDBLinkEntity.VALIDFLAG == "2")
            {
                dbContext.DisableDbLog();
            }
            return dbContext;
        }

        /// <summary>
        /// 获取变量前缀正则
        /// </summary>
        public static string GetParamRegex(this IDbContext source)
        {
            source.CheckNotNull(nameof(source));
            return @"(\:|\?|\@)";
        }

        private static readonly List<string> IDPropertys = new() { "MODIFYUSERID", "MODIFY_USERID" };
        private static readonly List<string> NamePropertys = new() { "MODIFYUSER" };
        private static readonly List<string> DatePropertys = new() { "MODIFYDATE" };
        private static readonly List<string> CreateIDPropertys = new() { "CREATEUSERID", "CREATE_USERID" };
        private static readonly List<string> CreateNamePropertys = new() { "CREATEUSER" };
        private static readonly List<string> CreateDatePropertys = new() { "CREATEDATE" };

        /// <summary>
        /// 通用保存
        /// </summary>
        /// <typeparam name="T">实体</typeparam>
        /// <param name="source">IDbContext</param>
        /// <param name="adds">新增数据</param>
        /// <param name="updates">更新数据</param>
        /// <param name="deletes">删除数据</param>
        /// <param name="updateFields">可更新字段</param>
        /// <param name="updateCondition">更新条件</param>
        /// <param name="beforeAdd">新增前委托</param>
        /// <param name="beforeUpdate">修改前委托</param>
        /// <param name="beforeDelete">删除前委托</param>
        /// <param name="IsSoftDelete">是否软删除</param>
        /// <param name="beforeSave">保存前委托</param>
        /// <param name="afterSave">保存后委托</param
        /// <param name="isLog">是否记录日志 默认记录</param>
        /// <returns></returns>
        public static async Task<AjaxResult> SaveEntityAnsyc<T>(this IDbContext source,
            SaveRequest<T> request,
            Expression<Func<T, object>> updateFields,
            Func<T, Expression<Func<T, bool>>> updateCondition,
            Func<T, Task> beforeAdd = null, Func<T, Task> beforeUpdate = null,
            Func<T, Task> beforeDelete = null, bool IsSoftDelete = false,
            Func<List<T>, List<T>, List<T>, Task> beforeSave = null,
            Func<List<T>, List<T>, List<T>, Task> afterSave = null)
        {
            var canTransationOper = false;
            try
            {
                request.Added ??= new List<T>();
                request.Updated ??= new List<T>();
                request.Deleted ??= new List<T>();
                canTransationOper = !source.Session.IsInTransaction;
                if (canTransationOper)
                {
                    source.Session.BeginTransaction();
                }
                if (beforeSave != null) await beforeSave(request.Added, request.Updated, request.Deleted);
                int row;
                var typeDescriptor = EntityTypeContainer.GetDescriptor(typeof(T));
                var hasPrimaryKey = typeDescriptor.HasPrimaryKey();
                var user = HttpContext.Current.GetCurrentUserOrDefault();
                var sysdate = await source.GetSysdate();
                Type idType = user.UserID.GetType().GetUnNullableType(), nameType = user.Display.GetType().GetUnNullableType(), dateType = sysdate.GetType().GetUnNullableType();
                var idPropertys = typeDescriptor.PrimitivePropertyDescriptors.Where(c => IDPropertys.Contains(c.Property.Name) && c.PropertyType.GetUnNullableType() == idType).ToList();
                var namePropertys = typeDescriptor.PrimitivePropertyDescriptors.Where(c => NamePropertys.Contains(c.Property.Name) && c.PropertyType.GetUnNullableType() == nameType).ToList();
                var datePropertys = typeDescriptor.PrimitivePropertyDescriptors.Where(c => DatePropertys.Contains(c.Property.Name) && c.PropertyType.GetUnNullableType() == dateType).ToList();
                foreach (var entity in request.Deleted)
                {
                    if (IsSoftDelete)
                    {
                        source.TrackEntity(entity);
                        idPropertys.ForEach(c => { c.SetValue(entity, user.UserID); });
                        namePropertys.ForEach(c => { c.SetValue(entity, user.Display); });
                        datePropertys.ForEach(c => { c.SetValue(entity, sysdate); });
                        if (beforeDelete != null) await beforeDelete(entity);
                        row = hasPrimaryKey ? await source.UpdateAsync(entity) : await source.UpdateAsync(entity, updateCondition(entity));
                    }
                    else
                    {
                        if (beforeDelete != null) await beforeDelete(entity);
                        row = hasPrimaryKey ? await source.DeleteAsync(entity) : await source.DeleteAsync(updateCondition(entity));
                    }

                    if (row != 1)
                    {
                        return AjaxResult.Error("删除记录出错");
                    }
                }
                if (request.Added.Count > 0)
                {
                    var createIDPropertys = typeDescriptor.PrimitivePropertyDescriptors.Where(c => CreateIDPropertys.Contains(c.Property.Name) && c.PropertyType.GetUnNullableType() == idType).ToList();
                    var createNamePropertys = typeDescriptor.PrimitivePropertyDescriptors.Where(c => CreateNamePropertys.Contains(c.Property.Name) && c.PropertyType.GetUnNullableType() == nameType).ToList();
                    var createDatePropertys = typeDescriptor.PrimitivePropertyDescriptors.Where(c => CreateDatePropertys.Contains(c.Property.Name) && c.PropertyType.GetUnNullableType() == dateType).ToList();
                    foreach (var entity in request.Added)
                    {
                        idPropertys.Concat(createIDPropertys).ForEach(c => { c.SetValue(entity, user.UserID); });
                        namePropertys.Concat(createNamePropertys).ForEach(c => { c.SetValue(entity, user.Display); });
                        datePropertys.Concat(createDatePropertys).ForEach(c => { c.SetValue(entity, sysdate); });
                        if (beforeAdd != null) await beforeAdd(entity);
                        await source.InsertAsync(entity);
                    }
                }
                for (int i = 0; i < request.Updated.Count; i++)
                {
                    var entity = request.Updated[i];
                    var orgin = request.Original.Count > i ? request.Original[i] : default;
                    source.TrackEntity(entity);
                    source.SetChangedFields(entity, updateFields, orgin);
                    idPropertys.ForEach(c => { c.SetValue(entity, user.UserID); });
                    namePropertys.ForEach(c => { c.SetValue(entity, user.Display); });
                    datePropertys.ForEach(c => { c.SetValue(entity, sysdate); });
                    if (beforeUpdate != null) await beforeUpdate(entity);
                    row = await source.UpdateAsync(entity, updateCondition(entity));
                    if (row != 1)
                    {
                        return AjaxResult.Error("修改记录出错");
                    }
                }
                if (afterSave != null) await afterSave(request.Added, request.Updated, request.Deleted);
                if (canTransationOper) source.Session.CommitTransaction();
                return AjaxResult.Success("保存成功");
            }
            catch (Exception ex)
            {
                return AjaxResult.Error($"系统错误：{ex.FormatMessage()}");
            }
            finally
            {
                if (canTransationOper && source.Session.IsInTransaction)
                {
                    source.Session.RollbackTransaction();
                }
            }
        }

        /// <summary>
        /// 数据库日志
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="op"></param>
        /// <param name="entity"></param>
        /// <param name="original"></param>
        /// <param name="curoper"></param>
        /// <returns></returns>
        public static async Task DBLog<TEntity>(this IDbContext source, string op, TEntity entity, TEntity original = default, string curoper = "")
        {
            var type = typeof(TEntity);
            var typeDescriptor = EntityTypeContainer.GetDescriptor(type);
            var key = source.GetPrimaryValues(entity, typeDescriptor).Values.ToStr(",");
            var detail = source.GetLogDetail(entity, original, typeDescriptor);
            var table = typeDescriptor.Definition.GetDescription();
            await source.DBLog($"{table} {op}", key, detail, curoper);
        }

        private static readonly LogPath _logPath = new("SQL");

        /// <summary>
        /// 数据库日志
        /// </summary>
        /// <param name="source"></param>
        /// <param name="op"></param>
        /// <param name="keyWord"></param>
        /// <param name="detail"></param>
        /// <param name="curoper"></param>
        /// <returns></returns>
        public static async Task DBLog(this IDbContext source, string op, string keyWord, string detail = "", string curoper = "")
        {
            var user = HttpContext.Current.GetCurrentUserOrDefault();
            detail ??= "";
            detail += $"{Environment.NewLine}IP:{user.IP}";
            var entity = new SYS_LOG
            {
                LOGTYPE = op,
                LOGNO = keyWord.SubStr(0, 100, true),
                LOGSUMMARY = detail.SubStr(0, 2000)
            };
            if (entity.LOGSUMMARY.Length == detail.Length)
            {
                entity.ISCLOB = "0";
            }
            else
            {
                entity.ISCLOB = "1";
                entity.LOGDETAIL = detail;
            }
            entity.LOGDATE = await source.GetSysdate();
            if (string.IsNullOrEmpty(curoper))
            {
                curoper = user.UserName;
            }
            if (string.IsNullOrEmpty(curoper))
            {
                curoper = user.IP;
            }
            entity.LOGOP = curoper;
            entity.APPNAME = user.MenuAppname;
            await source.InsertAsync(entity);
            var logger = HttpContext.RequestServices.GetService<ILogger<SYS_LOG>>();//同时写入日志系统
            logger.LogInformation(_logPath, "{LogNO}  {@entity}", entity.LOGNO, entity);
        }

        /// <summary>
        /// 获取日志详情
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="source"></param>
        /// <param name="entity"></param>
        /// <param name="original"></param>
        /// <param name="typeDescriptor"></param>
        /// <returns></returns>
        public static string GetLogDetail<TEntity>(this IDbContext source, TEntity entity, TEntity original = default, TypeDescriptor typeDescriptor = null)
        {
            if (source == null) return "";
            var builder = new StringBuilder();
            if (typeDescriptor == null) EntityTypeContainer.GetDescriptor(typeof(TEntity));
            foreach (PrimitivePropertyDescriptor propertyDescriptor in typeDescriptor.PrimitivePropertyDescriptors)
            {
                var value = propertyDescriptor.GetValue(entity)?.ToString();
                var originalValue = original == null ? null : propertyDescriptor.GetValue(original)?.ToString();
                if (value != originalValue)
                {
                    var key = propertyDescriptor.GetDescription();
                    if (!string.IsNullOrEmpty(originalValue)) originalValue = $"{originalValue}→";
                    builder.Append($"{key}:{originalValue}{value}{Environment.NewLine}");
                }
            }
            return builder.ToString().TrimEnd(Environment.NewLine.ToCharArray());
        }

        /// <summary>
        /// 用户日志
        /// </summary>
        /// <returns></returns>
        public static async Task UserLogAsync(this IDbContext source, string type, string title, string summary, UserSession user = null)
        {
            user ??= HttpContext.Current.GetCurrentUserOrDefault();
            var userLog = new CF_USER_LOG()
            {
                ID = GuidHelper.NewSnowflakeId(),
                LOGTYPE = type,
                LOGTITLE = title,
                LOGSUMMARY = summary,
                LOGINNAME = user.UserName,
                LOGINDATE = await source.GetSysdate(),
                IP = user.IP,
                UA = user.UserAgent,
                APPNAME = user.MenuAppname
            };
            await source.InsertAsync(userLog);
        }

        /// <summary>
        /// 获取树形节点的节点值
        /// </summary>
        /// <param name="source">事务</param>
        /// <param name="tableName">表名</param>
        /// <param name="parentNode">父节点值</param>
        /// <param name="where">条件</param>
        /// <param name="length">长度</param>
        /// <param name="nodeName">节点字段名</param>
        /// <returns></returns>
        public static async Task<string> GetTreeNode(this IDbContext source, string tableName, string parentNode, string where = "", int length = 3, string nodeName = "TreeNode")
        {
            parentNode ??= "";
            where = (where ?? "").Trim();
            if (!string.IsNullOrEmpty(where) && !where.StartsWith("and", false, System.Globalization.CultureInfo.CurrentCulture))
            {
                where = "and " + where;
            }
            var sql = $"select {nodeName} from {tableName} where {nodeName} like '{parentNode}%' and len({nodeName}) = {parentNode.Length + length} {where}";
            var list = (await source.SqlQueryAsync<string>(sql)).Select(c =>
            {
                return c.Remove(0, parentNode.Length).CastTo<int>();
            }).ToList();
            var pow = Math.Pow(10, length);
            var maxNode = (list.Count == 0 ? 0 : list.Max()) + 1;
            if (maxNode >= pow)
            {
                for (var i = 1; i < pow; i++)
                {
                    if (!list.Contains(i))
                    {
                        return $"{parentNode}{i.ToString(pow.CastTo<string>()[1..])}";
                    }
                }
            }
            else
            {
                return $"{parentNode}{maxNode.ToString(pow.CastTo<string>()[1..])}";
            }
            throw new MessageException("序号已经被用完");
        }

        #region 数据库日志

        /// <summary>
        /// 不写入数据库日志
        /// </summary>
        /// <returns></returns>
        public static bool NotDbLog(this TypeDefinition source)
        {
            return source.Annotations.Any(c => c is NotDbLogAttribute);
        }

        /// <summary>
        /// 不写入数据库日志
        /// </summary>
        /// <returns></returns>
        public static async Task NotDbLog(this IDbContext source, Func<Task> func)
        {
            source.DisableDbLog();
            await func();
            source.EnableDbLog();
        }

        /// <summary>
        /// 关闭数据库日志标识
        /// </summary>
        private static readonly string _dbLogKey = "NotDbLog";

        /// <summary>
        /// 是否关闭数据库日志
        /// </summary>
        /// <returns></returns>
        public static bool IsDisableDbLog(this IDbContext source)
        {
            return source.GetItem<bool>(_dbLogKey);
        }

        /// <summary>
        /// 关闭数据库日志
        /// </summary>
        /// <returns></returns>
        public static void DisableDbLog(this IDbContext source)
        {
            source.SetItem(_dbLogKey, true);
        }

        /// <summary>
        /// 启用数据库日志
        /// </summary>
        /// <returns></returns>
        public static void EnableDbLog(this IDbContext source)
        {
            source.Items.Remove(_dbLogKey);
        }

        #endregion 数据库日志

        /// <summary>
        /// 注册过滤器
        /// </summary>
        public static void AddEntityTypeBuilder(this Assembly source)
        {
            var type = typeof(InternalEntityTypeBuilder<>);
            source.GetTypes().Where(t => type.IsGenericAssignableFrom(t) && !t.IsAbstract).ForEach(c =>//动态加入全局过滤
            {
                DbConfiguration.UseTypeBuilders(c);
            });
        }
    }
}