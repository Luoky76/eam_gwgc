using Chloe.Extensions;
using Chloe.Infrastructure;
using Gksyb.Core.Filter;
using Gksyb.Model.Filter;
using Gksyb.Model.Grid;
using System.Collections;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Gksyb.Core.Grid
{
    /// <summary>
    /// 扩展GridDataBulider
    /// </summary>
    public static class GridDataBuliderExtension
    {
        private static readonly string[] _whereConstants = { "'@#' = '@#'", "N'@#' = N'@#'" };

        /// <summary>
        /// 获取表格数据
        /// </summary>
        /// <param name="source"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static async Task<GridData<List<T>>> GetGridData<T>(this IDbContext source, GridRequest request, Action<IList<DbParam>> action = null)
        {
            var whereTranslator = request.GetFilterTranslator();
            request.Where = whereTranslator.CommandText;
            foreach (var whereConstant in _whereConstants)
            {
                if (request.View.Contains(whereConstant))
                {
                    request.View = request.View.Replace(whereConstant, $"({request.Where})");
                    request.Where = null;
                    break;
                }
            }
            if (request.Where != null)
            {
                if (Regex.IsMatch(request.View, @"(\s+where\s+)", RegexOptions.IgnoreCase)) request.View = $"{request.View} AND ({request.Where})";
                else request.View = $"{request.View} where ({request.Where})";
            }
            if (Regex.IsMatch(request.View, @"{(\w+)}"))
            {
                var paramPrefix = source.GetParamPrefix();
                var matchCollection = Regex.Matches(request.View, @"{(\w+)}");
                foreach (Match mactch in matchCollection.Cast<Match>())
                {
                    var name = mactch.Value;
                    object value = DBNull.Value;
                    if (FilterParmMatch.CurrentParmMatch.ContainsKey(name))
                    {
                        value = FilterParmMatch.CurrentParmMatch[name]();
                        if (value is IEnumerable && value is not string)
                        {
                            value = string.Join(",", (value as IEnumerable).Cast<object>().Select(c => string.Format("'{0}'", c.CastTo<string>())));
                            request.View = request.View.Replace(name, value as string);
                            continue;
                        }
                    }
                    name = Regex.Replace(name, @"[{}]", "");
                    if (whereTranslator.Parms.Any(c => c.Name == name)) continue;
                    var dbParam = new DbParam(name.TrimStart(paramPrefix.ToCharArray()), value);
                    if (value == DBNull.Value) dbParam.Type = typeof(string);
                    whereTranslator.Parms.Add(dbParam);
                }
                request.View = Regex.Replace(request.View, @"{(\w+)}", $"{paramPrefix}$1");
            }
            action?.Invoke(whereTranslator.Parms);
            int? total = null;
            if (request.IsTotal)
            {
                total = (await source.Session.ExecuteScalarAsync($"select count(1) from ({request.View}) tmptableinner", whereTranslator.Parms)).CastTo<int>();
            }
            var order = request.HasSort ? $"ORDER BY {request.SortName} {request.OrderBy}" : "";
            string sql;
            if (request.Page.HasValue && request.PageSize.HasValue)
            {
                sql = source.GetPageSql();
                sql = sql.Replace("{N}", ((request.Page - 1) * request.PageSize).ToString());
                sql = sql.Replace("{M}", request.PageSize.ToString());
                sql = sql.Replace("{Order}", order);
                sql = sql.Replace("{View}", request.View);
            }
            else
            {
                sql = $"{request.View} {order}";
            }
            var list = await source.SqlQueryAsync<T>(sql, whereTranslator.Parms);
            return new GridData<List<T>>() { Rows = list, Total = total };
        }

        /// <summary>
        /// 获取GridData
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="request"></param>
        /// <param name="paraname"></param>
        /// <returns></returns>
        public static async Task<GridData> GetGridData<T>(this IQuery<T> source, GridRequest request, string paraname = null)
        {
            var gridData = await source.GetGridDataList(request, paraname);
            return new GridData()
            {
                Rows = gridData.Rows,
                Total = gridData.Total
            };
        }

        /// <summary>
        /// 获取GridData
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="request"></param>
        /// <param name="paraname"></param>
        /// <returns></returns>
        public static async Task<GridData<IList>> GetGridDataList<T>(this IQuery<T> source, GridRequest request, string paraname = null)
        {
            var gridData = await source.GetGridDataInner(request, paraname);
            if (!gridData.Total.HasValue) gridData.Total = gridData.Rows.Count;
            return gridData;
        }

        /// <summary>
        /// 获取GridData
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="request"></param>
        /// <param name="paraname"></param>
        /// <returns></returns>
        private static async Task<GridData<IList>> GetGridDataInner<T>(this IQuery<T> source, GridRequest request, string paraname)
        {
            int? total = null;
            if (request != null)
            {
                var hasColumns = request.HasColumn;
                var hasSort = request.HasSort;
                var type = typeof(T);
                paraname ??= type.Name;
                var parameterExpressions = new ParameterExpression[] { Expression.Parameter(typeof(T), paraname) };
                source = source.Where(request.Where, parameterExpressions);
                if (!string.IsNullOrEmpty(request.GroupBy))//分组处理 限制非常大请考虑直接在Query查询
                {
                    var groupExpression = DynamicExpressionParser.ParseLambda(parameterExpressions, null, $"new ({request.GroupBy})");
                    if (request.IsTotal)
                    {
                        total = await source.GroupBy(groupExpression).Select(DynamicExpressionParser.ParseLambda(parameterExpressions, null, $"new ({request.GroupBy})")).CountAsync();
                    }
                    if (hasSort) source = source.OrderBy($"{request.SortName} {request.OrderBy}");
                    var groupQuery = source.GroupBy(groupExpression);
                    IQuery<object> query = groupQuery.Select(DynamicExpressionParser.ParseLambda(parameterExpressions, null, $"new ({(hasColumns ? request.Columns : request.GroupBy)})"));
                    if (request.Page.HasValue && request.PageSize.HasValue)
                    {
                        query = query.TakePage(request.Page.Value, request.PageSize.Value);
                    }
                    return new GridData<IList>
                    {
                        Rows = await query.ToListAsync(),
                        Total = total
                    };
                }
                if (request.IsTotal)
                {
                    total = await source.CountAsync();
                }
                if (hasSort) source = source.OrderBy($"{request.SortName} {request.OrderBy}");
                if (request.Page.HasValue && request.PageSize.HasValue)
                {
                    source = source.TakePage(request.Page.Value, request.PageSize.Value);
                }
                if (hasColumns)
                {
                    return new GridData<IList>
                    {
                        Rows = await source.SelectEx(DynamicExpressionParser.ParseLambda(parameterExpressions, null, $"new ({request.Columns})")).ToListAsync(),
                        Total = total
                    };
                }
            }
            return new GridData<IList>
            {
                Rows = await source.ToListAsync(),
                Total = total
            };
        }

        /// <summary>
        /// 字符串条件过滤
        /// </summary>
        public static IQuery<T> Where<T>(this IQuery<T> source, string where, ParameterExpression[] parameterExpressions = null)
        {
            if (string.IsNullOrWhiteSpace(where)) return source;
            return source.Where(ToExpression<T>(where, parameterExpressions));
        }

        /// <summary>
        /// 获取where条件
        /// </summary>
        public static Expression<Func<T, bool>> GetWhereExpression<T>(this GridRequest request, ParameterExpression[] parameterExpressions = null)
        {
            return ToExpression<T>(request.Where, parameterExpressions);
        }

        /// <summary>
        /// 字符串转表达式
        /// </summary>
        private static Expression<Func<T, bool>> ToExpression<T>(string where, ParameterExpression[] parameterExpressions = null)
        {
            if (string.IsNullOrWhiteSpace(where)) return null;
            if (parameterExpressions == null)
            {
                var type = typeof(T);
                parameterExpressions = new ParameterExpression[] { Expression.Parameter(type, type.Name) };
            }
            var whereTranslator = new FilterTranslatorLinq(EntityTypeContainer.GetDescriptor(typeof(T)))
            {
                Group = where.ToObject<FilterGroup>()
            };
            whereTranslator.Group?.Check();
            whereTranslator.Translate();
            var expression = whereTranslator.CommandText;
            var values = whereTranslator.Parms.Select(c =>
            {
                return ExpressionExtension.MakeWrapperAccess(c.Value, c.Value == null ? null : c.Type);
            }).ToArray();
            var exp = DynamicExpressionParser.ParseLambda(parameterExpressions, typeof(bool), expression, values);
            return (Expression<Func<T, bool>>)exp;
        }

        /// <summary>
        /// 获取查询条件
        /// </summary>
        /// <param name="source"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public static FilterTranslator GetFilterTranslator(this GridRequest request)
        {
            var whereTranslator = new FilterTranslator();
            if (!string.IsNullOrWhiteSpace(request.Where))
            {
                whereTranslator.Group = request.Where.ToObject<FilterGroup>();
                whereTranslator.Group?.Check();
            }
            whereTranslator.Translate();
            return whereTranslator;
        }

        /// <summary>
        /// 获取指定名称的规则的值
        /// </summary>
        public static T GetRuleValue<T>(this GridRequest request, string name)
        {
            if (string.IsNullOrWhiteSpace(request.Where)) return default;
            var rule = request.Where.ToObject<FilterGroup>()?.GetRule(name);
            if (rule == null) return default;
            return rule.Value.CastTo<T>(default);
        }
    }
}