using Chloe.Annotations;
using Chloe.Descriptors;
using Chloe.Entity;
using Chloe.Infrastructure;
using Gksyb.Common.Data;
using Gksyb.Common.Static;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.Tree;
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
        /// <param name="isSoftDelete">是否软删除</param>
        /// <param name="beforeSave">保存前委托</param>
        /// <param name="afterSave">保存后委托</param
        /// <param name="orgin">如果没有原始值，则主动从数据库获取</param>
        /// <param name="beforeUpdate2">修改前委托(带旧值)</param>
        /// <param name="autoSet">自动设置 操作人和操作时间</param>
        /// <returns></returns>
        public static async Task<AjaxResult> SaveEntityAnsyc<T>(this IDbContext source,
            SaveRequest<T> request,
            Expression<Func<T, object>> updateFields,
            Func<T, Expression<Func<T, bool>>> updateCondition,
            Func<T, Task> beforeAdd = null, Func<T, Task> beforeUpdate = null,
            Func<T, Task> beforeDelete = null, bool isSoftDelete = false,
            Func<List<T>, List<T>, List<T>, Task> beforeSave = null,
            Func<List<T>, List<T>, List<T>, Task> afterSave = null, bool orgin = false, Func<T, T, Task> beforeUpdate2 = null, bool autoSet = true)
        {
            var doTransation = false;
            try
            {
                request.Added ??= new List<T>();
                request.Updated ??= new List<T>();
                request.Original ??= new List<T>();
                request.Deleted ??= new List<T>();
                doTransation = !source.Session.IsInTransaction;
                if (doTransation) source.Session.BeginTransaction();
                if (beforeSave != null) await beforeSave(request.Added, request.Updated, request.Deleted);
                int row;
                var typeDescriptor = EntityTypeContainer.GetDescriptor(typeof(T));
                var hasPrimaryKey = typeDescriptor.HasPrimaryKey();
                var user = HttpContext.Current.GetCurrentUserOrDefault();
                var sysdate = await source.GetSysdate();
                Type idType = user.UserID.GetType().GetUnNullableType(), nameType = user.Display.GetType().GetUnNullableType(), dateType = sysdate.GetType().GetUnNullableType();
                var idPropertys = typeDescriptor.PrimitivePropertyDescriptors.Where(c => IDPropertys.Any(a => a.EqualsTo(c.Property.Name, true)) && c.PropertyType.GetUnNullableType() == idType).ToList();
                var namePropertys = typeDescriptor.PrimitivePropertyDescriptors.Where(c => NamePropertys.Any(a => a.EqualsTo(c.Property.Name, true)) && c.PropertyType.GetUnNullableType() == nameType).ToList();
                var datePropertys = typeDescriptor.PrimitivePropertyDescriptors.Where(c => DatePropertys.Any(a => a.EqualsTo(c.Property.Name, true)) && c.PropertyType.GetUnNullableType() == dateType).ToList();
                foreach (var entity in request.Deleted)
                {
                    if (isSoftDelete)
                    {
                        source.TrackEntity(entity);
                        if (autoSet)
                        {
                            idPropertys.ForEach(c => { c.SetValue(entity, user.UserID); });
                            namePropertys.ForEach(c => { c.SetValue(entity, user.Display); });
                        }
                        if (beforeDelete != null) await beforeDelete(entity);
                        if (autoSet) datePropertys.ForEach(c => { c.SetValue(entity, sysdate); });
                        row = hasPrimaryKey ? await source.UpdateAsync(entity) : await source.UpdateAsync(entity, updateCondition(entity));
                    }
                    else
                    {
                        if (beforeDelete != null) await beforeDelete(entity);
                        row = hasPrimaryKey ? await source.DeleteAsync(entity) : await source.DeleteAsync(updateCondition(entity));
                    }
                    MessageException.ThrowIf(row != 1, "删除记录出错");
                }
                if (request.Added.Count > 0)
                {
                    var createIDPropertys = typeDescriptor.PrimitivePropertyDescriptors.Where(c => CreateIDPropertys.Any(a => a.EqualsTo(c.Property.Name, true)) && c.PropertyType.GetUnNullableType() == idType).ToList();
                    var createNamePropertys = typeDescriptor.PrimitivePropertyDescriptors.Where(c => CreateNamePropertys.Any(a => a.EqualsTo(c.Property.Name, true)) && c.PropertyType.GetUnNullableType() == nameType).ToList();
                    var createDatePropertys = typeDescriptor.PrimitivePropertyDescriptors.Where(c => CreateDatePropertys.Any(a => a.EqualsTo(c.Property.Name, true)) && c.PropertyType.GetUnNullableType() == dateType).ToList();
                    foreach (var entity in request.Added)
                    {
                        if (autoSet)
                        {
                            idPropertys.Concat(createIDPropertys).ForEach(c => { c.SetValue(entity, user.UserID); });
                            namePropertys.Concat(createNamePropertys).ForEach(c => { c.SetValue(entity, user.Display); });
                            datePropertys.Concat(createDatePropertys).ForEach(c => { c.SetValue(entity, sysdate); });
                        }
                        if (beforeAdd != null) await beforeAdd(entity);
                        await source.InsertAsync(entity);
                    }
                }
                for (int i = 0; i < request.Updated.Count; i++)
                {
                    var entity = request.Updated[i];
                    var old = request.Original.Count > i ? request.Original[i] : (orgin ? await source.Query<T>().Where(updateCondition(entity)).FirstOrDefaultAsync() : default);
                    if (orgin && !request.Original.Contains(old)) request.Original.Add(old);
                    source.TrackEntity(entity);
                    source.SetChangedFields(entity, updateFields, old);
                    if (autoSet)
                    {
                        idPropertys.ForEach(c => { c.SetValue(entity, user.UserID); });
                        namePropertys.ForEach(c => { c.SetValue(entity, user.Display); });
                    }
                    if (beforeUpdate != null) await beforeUpdate(entity);
                    if (autoSet) datePropertys.ForEach(c => { c.SetValue(entity, sysdate); });
                    if (beforeUpdate2 != null) await beforeUpdate2(entity, old);
                    row = await source.UpdateAsync(entity, updateCondition(orgin ? old : entity));
                    MessageException.ThrowIf(row != 1, "修改记录出错");
                }
                if (afterSave != null) await afterSave(request.Added, request.Updated, request.Deleted);
                if (doTransation) source.Session.CommitTransaction();
                return AjaxResult.Success("保存成功");
            }
            finally
            {
                if (doTransation && source.Session.IsInTransaction)
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
            keyWord = keyWord.SubStr(0, 100, true);
            var summary = detail.SubStr(0, 2000);
            var isClob = "1";
            if (summary.Length == detail.Length)
            {
                isClob = "0";
                detail = null;
            }
            if (string.IsNullOrEmpty(curoper))
            {
                curoper = user.UserName;
            }
            if (string.IsNullOrEmpty(curoper))
            {
                curoper = user.IP;
            }
            await source.NotSqlLog(async () =>
            {
                await source.InsertAsync(() => new SYS_LOG()
                {
                    LOGTYPE = op,
                    LOGNO = keyWord,
                    LOGSUMMARY = summary,
                    ISCLOB = isClob,
                    LOGDETAIL = detail,
                    LOGDATE = DateTime.Now,
                    LOGOP = curoper,
                    APPNAME = user.MenuAppname
                });
            });
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
                LOGSUMMARY = (summary ?? "").SubStr(0, 2000),
                LOGINNAME = user.UserName,
                LOGINDATE = await source.GetSysdate(),
                IP = user.IP,
                UA = user.UserAgent,
                APPNAME = user.MenuAppname
            };
            await source.InsertAsync(userLog);
        }

        /// <summary>
        /// 树形节点处理
        /// </summary>
        public static async Task<string> TreeHandle<T>(this IDbContext source, T entity, string oldTreeNode, Expression<Func<T, bool>> predicate = null, int length = 3) where T : ITreeable<string>, new()
        {
            return await source.TreeHandle<T, string>(entity, oldTreeNode, predicate, length);
        }

        /// <summary>
        /// 树形节点处理
        /// </summary>
        public static async Task<string> TreeHandle<T, T1>(this IDbContext source, T entity, string oldTreeNode, Expression<Func<T, bool>> predicate = null, int length = 3) where T : ITreeable<T1>, new() where T1 : IEquatable<T1>
        {
            var parentNode = "";
            if (entity.PARENTID is string parentId ? !string.IsNullOrWhiteSpace(parentId) : entity.PARENTID != null && !entity.PARENTID.Equals(0))//有父节点
            {
                parentNode = await source.Query<T>().Where(c => c.ID.Equals(entity.PARENTID)).Select(c => c.TREENODE).FirstOrDefaultAsync();
                if (string.IsNullOrWhiteSpace(parentNode)) return parentNode;
            }
            entity.TREENODE = await source.GetTreeNode<T, T1>(parentNode, predicate, length);
            if (string.IsNullOrWhiteSpace(oldTreeNode)) return entity.TREENODE;
            if (parentNode.StartsWith(oldTreeNode))
            {
                throw new MessageException("层级关系错误，上级不能直接改成下级");
            }
            var childs = (await source.Query<T>().Where(c => c.TREENODE.StartsWith(oldTreeNode) && !c.ID.Equals(entity.ID)).ToListAsync()).OrderBy(c => c.TREENODE.Length);
            foreach (var child in childs)
            {
                var parent = childs.FirstOrDefault(c => c.ID.Equals(child.PARENTID)) ?? (new T() { TREENODE = entity.TREENODE });
                var corpPath = await source.GetTreeNode<T, T1>(parent.TREENODE, predicate, length);
                await source.UpdateAsync<T>(c => c.ID.Equals(child.ID), c => new T()
                {
                    TREENODE = corpPath
                });
            }
            return entity.TREENODE;
        }

        /// <summary>
        /// 获取树形节点的节点值
        /// </summary>
        private static async Task<string> GetTreeNode<T, T1>(this IDbContext source, string parentNode, Expression<Func<T, bool>> predicate = null, int length = 3) where T : ITreeable<T1>, new() where T1 : IEquatable<T1>
        {
            var len = parentNode.Length + length;
            var nodes = await source.Query<T>().Where(c => c.TREENODE.StartsWith(parentNode) && c.TREENODE.Length == len)
                .Where(predicate).Select(c => c.TREENODE).ToListAsync();
            var list = nodes.Select(c => c.Remove(0, parentNode.Length).CastTo<int>()).ToList();
            var maxNode = (list.Count == 0 ? 0 : list.Max()) + 1;
            var max = Math.Pow(10, length);
            var format = max.CastTo<string>()[1..];
            if (maxNode < max) return $"{parentNode}{maxNode.ToString(format)}";
            for (var i = 1; i < max; i++)
            {
                if (list.Contains(i)) continue;
                return $"{parentNode}{i.ToString(format)}";
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
        private const string _dbLogKey = "NotDbLog";

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