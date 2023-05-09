namespace Gksyb.Common
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// 遍历
        /// </summary>
        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            foreach (TSource element in source)
            {
                action(element);
            }
        }

        /// <summary>
        /// 遍历
        /// </summary>
        public static async Task ForEachAsync<TSource>(this IEnumerable<TSource> source, Func<TSource, Task> func)
        {
            foreach (TSource element in source)
            {
                await func(element);
            }
        }

        /// <summary>
        /// 是否包含元素
        /// </summary>
        /// <returns></returns>
        public static bool Contains<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            foreach (TSource element in source)
            {
                if (predicate(element)) return true;
            }
            return false;
        }

        /// <summary>
        /// 转字符串
        /// </summary>
        /// <returns></returns>
        public static string ToStr<TSource>(this IEnumerable<TSource> source, string separator)
        {
            return string.Join(separator, source.Select(c => c.CastTo<string>()));
        }

        /// <summary>
        /// 去空去重排序
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<TSource> DistinctAndOrderBy<TSource>(this IEnumerable<TSource> source)
        {
            return source.Where(a => !string.IsNullOrWhiteSpace(a.CastTo<string>(null))).Distinct().OrderBy(i => i);
        }

        /// <summary>
        /// 去空去重排序合并
        /// </summary>
        /// <returns></returns>
        public static string Join<TSource>(this IEnumerable<TSource> source, string separator = ",")
        {
            return source.DistinctAndOrderBy().ToStr(separator);
        }
    }
}