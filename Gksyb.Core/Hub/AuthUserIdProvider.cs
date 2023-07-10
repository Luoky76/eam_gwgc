using Gksyb.Core.Auth;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace Microsoft.AspNetCore.SignalR
{
    /// <summary>
    /// 用户验证
    /// </summary>
    public class AuthUserIdProvider : IUserIdProvider
    {
        /// <summary>
        /// 获取用户ID
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public string GetUserId(HubConnectionContext connection)
        {
            if (!string.IsNullOrWhiteSpace(connection.UserIdentifier)) return connection.UserIdentifier;
            var httpContext = connection.GetHttpContext();
            var token = httpContext.Request.Query["access_token"].ToString();
            if (!string.IsNullOrWhiteSpace(token))
            {
                var distributedCache = httpContext.RequestServices.GetService<IDistributedCache>();
                var userSession = distributedCache.Get<UserSession>(token, default);
                if (userSession != null)
                {
                    if (!userSession.Check(httpContext.Request, distributedCache)) return connection.UserIdentifier;
                    lock (httpContext)//source.Items不是多线程安全
                    {
                        httpContext.Items.Remove(nameof(UserSession));
                        httpContext.Items.Add(nameof(UserSession), userSession);
                        httpContext.User = userSession.ToClaimsPrincipal();
                    }
                    var list = (connection.User.Identities as List<ClaimsIdentity>);
                    list.Clear();
                    list.AddRange(httpContext.User.Identities);
                    connection.UserIdentifier = userSession.UserName;
                }
            }
            return connection.UserIdentifier;
        }
    }
}