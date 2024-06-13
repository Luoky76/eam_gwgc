using Gksyb.Common;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Microsoft.AspNetCore.Mvc
{
    /// <summary>
    /// 锁
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class SynchronizedAttribute : Attribute, IAsyncActionFilter
    {
        public string Key;

        /// <summary>
        /// 锁
        /// </summary>
        public SynchronizedAttribute()
        { }

        /// <summary>
        /// 锁
        /// </summary>
        /// <param name="key">指定key会变成分布式锁</param>
        public SynchronizedAttribute(string key) => Key = key;

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var key = Key;
            var httpContext = context.HttpContext;
            if (string.IsNullOrWhiteSpace(key))
            {
                var clientId = httpContext.GetClientID();
                if (string.IsNullOrWhiteSpace(clientId))
                {
                    clientId = CryptographyHelper.GetMd5($"{httpContext.Request.GetRealIP()}_{httpContext.Request.GetUserAgent()}");
                }
                key = httpContext.Request.Path.Value.TrimStart('/').TrimEnd('/').ToLower();
                key = $"{clientId}-{key}";
            }
            key = $"{key}-{nameof(SynchronizedAttribute)}";
            await DistributedLockHelper.LockAsync(key, 30 * 1000, async (isFail) =>
            {
                if (isFail)
                {
                    context.Result = new OkObjectResult(AjaxResult.Error("资源正忙，请稍后重试"));
                    return;
                }
                await next();
            }, 100, 300);
        }
    }
}