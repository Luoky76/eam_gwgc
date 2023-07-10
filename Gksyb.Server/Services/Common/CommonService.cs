using Gksyb.Common.Static;
using Gksyb.Core.Auth;
using Gksyb.Core.Common;
using Gksyb.Core.Filter;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using System.Collections;
using System.Collections.Concurrent;
using System.Data;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Gksyb.Server.Services.Common
{
    /// <summary>
    /// 查询视图
    /// </summary>
    public class CommonService : ICommonService
    {
        private readonly IDbContext _dbContext;
        private readonly IDistributedCache _distributedCache;
        private readonly string _appName;
        private readonly UserSession CurrentUser;

        /// <summary>
        /// 查询视图
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="distributedCache"></param>
        /// <param name="options"></param>
        public CommonService(IDbContext dbContext, IDistributedCache distributedCache, IOptions<SysContextOptions> options, UserSession currentUser)
        {
            _dbContext = dbContext;
            _distributedCache = distributedCache;
            _appName = options.Value.ConfigAppName ?? options.Value.AppName;
            CurrentUser = currentUser;
        }

        /// <summary>
        /// 获取系统日期
        /// </summary>
        /// <param name="request">请求参数</param>
        /// <returns></returns>
        public async Task<(DateTime, DateTime)> SysdateAsync(SysdateRequest request)
        {
            var sysdate = await _dbContext.GetSysdate();
            var datetime = DateTime.ParseExact(sysdate.Value.ToString(request.DateFormat), request.DateFormat, CultureInfo.CurrentCulture);
            var addDatetime = datetime;
            switch (request.DateAddType)
            {
                case "year":
                    addDatetime = datetime.AddYears(request.DateAdd.CastTo(0));
                    break;

                case "month":
                    addDatetime = datetime.AddMonths(request.DateAdd.CastTo(0));
                    break;

                case "day":
                    addDatetime = datetime.AddDays(request.DateAdd);
                    break;

                case "hour":
                    addDatetime = datetime.AddHours(request.DateAdd);
                    break;

                case "minute":
                    addDatetime = datetime.AddMinutes(request.DateAdd);
                    break;
            }
            return (datetime, addDatetime);
        }

        /// <summary>
        /// 根据配置获取json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<List<T>> JsonValueAsync<T>(QueryViewRequest request)
        {
            return await JsonValueAsync<T>(request, false);
        }

        /// <summary>
        /// 多线程执行 根据配置获取json
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        public async Task<IDictionary<string, object>> JsonValueMulAsync(IDictionary<string, IDictionary<string, object>> param)
        {
            var dicReturn = new ConcurrentDictionary<string, object>();
            await Parallel.ForEachAsync(param, async (dic, token) =>
            {
                try
                {
                    var list = await JsonValueAsync<dynamic>(new QueryViewRequest()
                    {
                        ViewName = dic.Key.Split("@#")[0],
                        Param = dic.Value,
                    }, true);
                    dicReturn.TryAdd(dic.Key, list);
                }
                catch (Exception ex)
                {
                    dicReturn.TryAdd(dic.Key, ex.ToString());
                }
            });
            return dicReturn;
        }

        /// <summary>
        /// 获取视图配置
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> QueryConfigAsync(string view)
        {
            var entity = await GetViewAsync(_dbContext, view)
                ?? throw new MessageException($"视图{view}不存在");
            return AjaxResult.Success(new { entity.FORM, entity.GRID, entity.Fields }, "");
        }

        /// <summary>
        /// 视图查询
        /// </summary>
        /// <returns></returns>
        public async Task<GridData<IList>> QueryAsync(GridRequest request)
        {
            var entity = await GetViewAsync(_dbContext, request.View)
                ?? throw new MessageException($"视图{request.View}不存在");
            var dbContextLind = await _dbContext.GetDbContext(entity.DataSource);
            request.View = entity.SEARCH;
            return await dbContextLind.GetGridData(request);
        }

        /// <summary>
        /// 根据配置获取json
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="isClone"></param>
        /// <returns></returns>
        private async Task<List<T>> JsonValueAsync<T>(QueryViewRequest request, bool isClone)
        {
            var view = request.ViewName;
            if (string.IsNullOrWhiteSpace(view)) throw new MessageException("请传递视图参数");
            await HttpContext.Current.ValidViewAsync(view);
            var dbContext = isClone ? _dbContext.Clone() : _dbContext;
            var entity = await GetViewAsync(dbContext, view)
                ?? throw new MessageException($"视图{view}不存在");
            var dbContextLind = await dbContext.GetDbContext(entity.DataSource);
            try
            {
                var paramPrefix = dbContextLind.GetParamPrefix();
                string paramname = string.Empty;
                IDictionary<string, object> dicParm = request.Param ?? new Dictionary<string, object>();
                var listPara = new List<DbParam>();
                List<T> list = null;
                view = entity.SEARCH;
                if (entity.HastParmMatch)//拦截规则
                {
                    var matchCollection = Regex.Matches(view, @"{(\w+)}");
                    foreach (var mactch in matchCollection.Cast<Match>())
                    {
                        var keyname = mactch.Value;
                        paramname = Regex.Replace(keyname, @"[{}]", "");
                        if (!dicParm.ContainsKey(paramname)) dicParm.TryAdd(paramname, keyname);
                    }
                    view = Regex.Replace(view, @"{(\w+)}", $"{paramPrefix}$1");
                }
                if (request.IsGridJson && !string.IsNullOrEmpty(entity.GRID)) //Grid有配置规则
                {
                    var parmMatch = new Dictionary<string, object>();
                    try
                    {
                        parmMatch = JSONHelper.FromJson<Dictionary<string, object>>(entity.GRID);
                    }
                    catch
                    {
                    }
                    foreach (var key in parmMatch.Keys)
                    {
                        if (!dicParm.ContainsKey(key)) dicParm.TryAdd(key, parmMatch[key]);
                    }
                }
                foreach (var key in dicParm.Keys)
                {
                    paramname = paramPrefix + key;
                    object value = dicParm[key];
                    if (value is string name)
                    {
                        if (FilterParmMatch.CurrentParmMatch.ContainsKey(name))
                        {
                            value = FilterParmMatch.CurrentParmMatch[name]();
                            if (value is IEnumerable && value is not string)
                            {
                                value = string.Join(",", (value as IEnumerable).Cast<object>().Select(c => string.Format("'{0}'", c.CastTo<string>())));
                                view = view.Replace(paramname, value as string);
                                continue;
                            }
                        }
                    }
                    listPara.Add(new DbParam(paramname.TrimStart(paramPrefix.ToCharArray()), value));
                }

                listPara.ForEach(c =>
                {
                    if (c.Value == null && c.Type == typeof(object))
                    {
                        c.Type = typeof(string);
                    }
                    if (c.Name.EndsWith("_in"))
                    {
                        c.Direction = ParamDirection.Input;
                    }
                    else if (c.Name.EndsWith("_out"))
                    {
                        c.Direction = ParamDirection.Output;
                    }
                    else if (c.Name.EndsWith("_cursor"))
                    {
                        c.ExplicitParameter = new OracleParameter(c.Name, OracleDbType.RefCursor, ParameterDirection.Output);
                    }
                });
                var arraySql = view.Split(';').Where(c => !string.IsNullOrWhiteSpace(c)).ToArray();
                await dbContextLind.UseTransactionAsync(async () =>
                {
                    for (int i = 0, j = (arraySql.Length - 1); i <= j; i++)
                    {
                        var commandType = CommandType.Text;
                        var sql = arraySql[i];
                        var parameters = listPara.FindAll(c => sql.Contains(c.Name));
                        if (sql.StartsWith("StoredProcedure"))
                        {
                            sql = sql.Replace("StoredProcedure", "");
                            sql = sql[..sql.IndexOf("(")].Trim();
                            commandType = CommandType.StoredProcedure;
                        }
                        parameters.ForEach(c =>
                        {
                            if (c.Direction == ParamDirection.InputOutput) c.Direction = ParamDirection.Input;
                            if (commandType == CommandType.StoredProcedure && c.Direction == ParamDirection.Input && !c.Name.EndsWith("_in"))
                            {
                                c.Direction = ParamDirection.InputOutput;
                            }
                        });
                        if (i < j)
                        {
                            await dbContextLind.Session.ExecuteNonQueryAsync(sql, commandType, parameters.ToArray());
                            continue;
                        }
                        try
                        {
                            string sortExp = (request.Sort ?? "").SqlFilter(80);
                            if (sortExp.HasValue())
                            {
                                var upperText = sql.ToUpper();
                                var lastIndex = upperText.LastIndexOf("ORDER BY ");
                                if (lastIndex >= 0 && upperText.IndexOf(" WHERE ", lastIndex) < 0)
                                {
                                    sql = sql[..lastIndex];
                                }
                                sql = "SELECT * FROM ({0}) tmptableinner ORDER BY {1}".FormatWith(sql, sortExp);
                            }
                        }
                        catch (Exception) { }
                        list = await dbContextLind.SqlQueryAsync<T>(sql, commandType, parameters.ToArray());
                    }
                });
                list ??= new List<T>();
                return list;
            }
            finally
            {
                if (dbContextLind != _dbContext) dbContextLind.Dispose();
                if (isClone) dbContext.Dispose();
            }
        }

        /// <summary>
        /// 获取视图
        /// </summary>
        /// <returns></returns>
        public async Task<QueryView> GetViewAsync(IDbContext dbContext, string view, string appname = null)
        {
            appname ??= _appName;
            var cacheName = $"{CachePrefix}{view}";
            var list = await _distributedCache.GetAsync<List<QueryView>>(cacheName, null);
            if (list == null)
            {
                list = await dbContext.Query<CF_CONFIGURATION>().Where(c => c.VIEWS == view).ToListAsync<QueryView>();
                list ??= new List<QueryView>();
                foreach (var c in list)
                {
                    await c.HandleAsync(dbContext);
                }
                await _distributedCache.SetAsync(cacheName, list, new DistributedCacheEntryOptions()
                {
                    SlidingExpiration = TimeSpan.FromHours(12)
                });
            }
            if (list.Count == 0) return null;
            return list.Find(c => c.APPNAME == appname) ?? list[0];
        }

        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <param name="view">视图名称</param>
        /// <returns></returns>
        public async Task RemoveCacheAsync(string view)
        {
            view = $"{CachePrefix}{view}";
            await _distributedCache.RemoveAsync(view);
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ClearAsync()
        {
            var list = await _dbContext.Query<CF_CONFIGURATION>().Select(c => c.VIEWS).ToListAsync();
            await list.ForEachAsync(async view =>
            {
                await RemoveCacheAsync(view);
            });
            return true;
        }

        public async Task<List<string>> GetDeptList(string dept)
        {
            //获取当前登录人所在公司
            var dept_id = dept;
            var sql = @"WITH RECURSIVE temp AS (
                           SELECT t.* FROM cf_dept t WHERE t.DEPT_ID = @dept_id
                           UNION ALL
                           SELECT t.* FROM cf_dept t INNER JOIN temp ON t.PARENT_ID = temp.DEPT_ID
                       )
                       SELECT * FROM temp";

            var list = await _dbContext.SqlQueryAsync<ComboxData>(sql, new
            {
                dept_id = dept_id
            });

            List<string> returnList = new List<string>();

            foreach (var item in list)
            {
                returnList.Add("," + item.ID.ToString() + ",");
            }

            return returnList;
        }




        //缓存前缀
        private static readonly string CachePrefix = "View_";
    }
}