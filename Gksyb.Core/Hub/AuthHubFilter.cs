using Gksyb.Core.Auth;
using System.Security.Claims;

namespace Microsoft.AspNetCore.SignalR
{
    /// <summary>
    /// Hub验证处理
    /// </summary>
    public class AuthHubFilter : IHubFilter
    {
        public async ValueTask<object> InvokeMethodAsync(HubInvocationContext invocationContext, Func<HubInvocationContext, ValueTask<object>> next)
        {
            var httpContext = invocationContext.Context.GetHttpContext();
            var isValid = await invocationContext.HubMethod.Valid(httpContext);
            if (isValid) return await next(invocationContext);
            await invocationContext.Hub.Clients.Caller.SendAsync("Excute", new ActionData()
            {
                Action = "Error",
                Data = "您无权进行此操作"
            });
            return Task.CompletedTask;
        }

        /// <summary>
        /// 开始连接
        /// </summary>
        /// <returns></returns>
        public async Task OnConnectedAsync(HubLifetimeContext context, Func<HubLifetimeContext, Task> next)
        {
            if (string.IsNullOrWhiteSpace(context.Context.UserIdentifier))
            {
                context.Context.Abort();
                return;
            }
            var group = context.Context.User.FindFirstValue(ClaimTypes.GroupSid);
            group = string.IsNullOrWhiteSpace(group) ? "Other" : group;
            await context.Hub.Groups.AddToGroupAsync(context.Context.ConnectionId, group);
            await next(context);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns></returns>
        public async Task OnDisconnectedAsync(
            HubLifetimeContext context, Exception exception, Func<HubLifetimeContext, Exception, Task> next)
        {
            var group = context.Context.User.FindFirstValue(ClaimTypes.GroupSid);
            group = string.IsNullOrWhiteSpace(group) ? "Other" : group;
            await context.Hub.Groups.RemoveFromGroupAsync(context.Context.ConnectionId, group);
            await next(context, exception);
        }
    }
}