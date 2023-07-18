namespace Microsoft.AspNetCore.Mvc
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class SkipXssFilterAttribute : Attribute
    {
        /// <summary>
        /// 跳过XSS过滤
        /// </summary>
        public SkipXssFilterAttribute()
        {
        }
    }
}