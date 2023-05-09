using Gksyb.Core.Auth;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Microsoft.AspNetCore.Mvc.Filters
{
    /// <summary>
    /// 锁过滤器
    /// </summary>
    public class SynchronizedFilter : IAsyncActionFilter
    {
        public SynchronizedFilter()
        {
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var description = (ControllerActionDescriptor)context.ActionDescriptor;
            var synchronizedAttribute = description.MethodInfo.GetAttribute<SynchronizedAttribute>();
            if (synchronizedAttribute == null)
            {
                await next();
                return;
            }
            var key = synchronizedAttribute.Key;
            var httpContext = context.HttpContext;
            if (string.IsNullOrWhiteSpace(key))
            {
                key = httpContext.Request.Path.Value.TrimStart('/').TrimEnd('/').ToLower();
                key = $"{httpContext.GetClientID()}-{key}";
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