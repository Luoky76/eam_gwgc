using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;

namespace Gksyb.Common
{
    public static class DynamicLinqExtensions
    {
        /// <summary>
        /// 正则替换{LeaveDays}变量
        /// </summary>
        public static string Replace(this string source, Func<string, object, string> func, Dictionary<string, object> formData, Dictionary<string, Func<object>> funcData = null, string pattern = null)
        {
            if (string.IsNullOrWhiteSpace(source)) return source;
            var data = formData.ToIgnoreCaseDictionary();
            if (data == null) return source;
            var funcs = funcData == null ? new Dictionary<string, Func<object>>(StringComparer.OrdinalIgnoreCase) : funcData.ToIgnoreCaseDictionary();
            pattern ??= @"{(\w+)}";
            return Regex.Replace(source, pattern, match =>
            {
                var key = match.Groups[1].Value;
                object value = default;
                if (data.ContainsKey(key))
                {
                    value = data[key];
                }
                else if (funcs.ContainsKey(match.Value))
                {
                    value = funcs[match.Value]();
                }
                if (func == null) return (value ?? "").ToString();
                return func(key, value);
            }, RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 动态执行脚本 {LeaveDays} > 2 ? 'flow1' : 'flow2'
        /// </summary>
        public static object Eval(this string source, Dictionary<string, object> formData, Dictionary<string, Func<object>> funcData = null)
        {
            var index = 0;
            var result = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var types = new List<object>();
            var expression = source.Replace((key, value) =>
            {
                var type = value.GetType();
                try
                {
                    type = typeof(Nullable<>).MakeGenericType(type);
                }
                catch
                {
                }
                types.Add(type);
                result.Add(key, value);
                return $"(As(it.{key},@{index++}))";
            }, formData, funcData);
            var queryable = (new List<Dictionary<string, object>>() { result }).AsQueryable();
            return queryable.Select(expression, types.ToArray()).ToDynamicList().First();
        }
    }
}