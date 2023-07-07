using Gksyb.Common;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// 防抖
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class DebounceAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly long _wait = 3000;

        public DebounceAttribute()
        { }

        /// <summary>
        /// 防抖
        /// </summary>
        /// <param name="wait">防抖时间</param>
        public DebounceAttribute(long wait) => _wait = wait;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            try
            {
                var httpContext = context.HttpContext;
                var key = httpContext.Request.Path.Value.TrimStart('/').TrimEnd('/').ToLower();
                key = $"{httpContext.GetClientID()}-{key}-{nameof(DebounceAttribute)}";
                await DistributedLockHelper.LockAsync(key, _wait, async (isFail) =>
                {
                    if (isFail) throw new MessageException("操作太过频繁，请稍后重试");
                    var distributedCache = httpContext.RequestServices.GetService<IDistributedCache>();
                    var isExcute = await distributedCache.GetStringAsync(key);
                    if (isExcute == "1") throw new MessageException("操作太过频繁，请稍后重试");
                    await distributedCache.SetStringAsync(key, "1", new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(_wait)
                    });
                }, 1);
            }
            catch (Exception ex)
            {
                context.Result = new OkObjectResult(AjaxResult.Error(ex.ToString()));
            }
        }
    }
}