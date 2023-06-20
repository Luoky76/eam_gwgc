using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Gksyb.Core.Auth
{
    /// <summary>
    /// 防抖
    /// </summary>
    public sealed class DebounceAttribute : AuthorizeAttribute
    {
        private readonly long _wait = 3000;

        public DebounceAttribute()
        {
        }

        /// <summary>
        /// 防抖
        /// </summary>
        /// <param name="wait">防抖时间</param>
        public DebounceAttribute(long wait)
        {
            _wait = wait;
        }

        public override async Task<bool> ValidAsync(HttpContext httpContext)
        {
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
            return true;
        }

        public override int GetOrder() => 1000;
    }
}