namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// 锁
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class SynchronizedAttribute : Attribute
    {
        /// <summary>
        /// 锁
        /// </summary>
        public SynchronizedAttribute()
        {
        }

        /// <summary>
        /// 锁
        /// </summary>
        /// <param name="key">指定key会变成分布式锁</param>
        public SynchronizedAttribute(string key)
        {
            Key = key;
        }

        public string Key;
    }
}