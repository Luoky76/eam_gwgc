using Gksyb.Core.Interfaces.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Gksyb.Core.Auth
{
    public static class HttpContextExtension
    {
        /// <summary>
        /// 根据token获取用户会话信息
        /// </summary>
        public static async Task<UserSession> GetCurrentUserAsync(this HttpContext source, string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;
            var distributedCache = source.RequestServices.GetService<IDistributedCache>();
            var userSession = await distributedCache.GetAsync<UserSession>(token, default);
            if (userSession == null) return userSession;
            if (!userSession.Check(source.Request, distributedCache)) return null;
            return userSession;
        }

        /// <summary>
        /// 获取用户会话信息
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static async Task<UserSession> GetCurrentUserAsync(this HttpContext source)
        {
            UserSession userSession = default;
            if (source.Items.TryGetValue(nameof(UserSession), out object value))
            {
                userSession = value as UserSession;
            }
            if (userSession != null) return userSession;
            var token = source.GetAuthToken();
            userSession = source.GetCurrentUserAsync(token).Result();
            if (userSession == null) return userSession;
            if (userSession.IsApi)
            {
                var service = source.RequestServices.GetRequiredService<IApiUserInfoService>();
                await service.FromRequestAsync(source.Request, userSession);
            }
            lock (source)//source.Items不是多线程安全
            {
                source.Items.Remove(nameof(UserSession));
                source.Items.Add(nameof(UserSession), userSession);
                source.User = userSession.ToClaimsPrincipal();
            }
            return userSession;
        }

        /// <summary>
        /// 获取用户会话信息 找不到返回空对象
        /// </summary>
        /// <returns></returns>
        public static UserSession GetCurrentUserOrDefault(this HttpContext source)
        {
            var user = source?.GetCurrentUserAsync().Result();
            if (user != null) return user;
            var ip = source == null ? Gksyb.Common.Static.HttpContext.AddressList.FirstOrDefault() : source.Request?.GetRealIP();
            return new UserSession()
            {
                RealName = ip,
                IP = ip
            };
        }

        /// <summary>
        /// 验证视图
        /// </summary>
        /// <returns></returns>
        public static async Task<string> ValidViewAsync(this HttpContext source, string view, bool throwEx = true)
        {
            view ??= "";
            if (view.EndsWith("Public")) return view;
            var isBaseAuth = false;
            if (view.EndsWith("Common")) isBaseAuth = true;
            var request = source?.Request;
            if (request == null) return view;
            var menuNo = request.GetRealUrl().GetParm("MenuNo");
            var referer = request.Headers[HeaderNames.Referer].ToString();
            if (menuNo.IsNullOrWhiteSpace()) menuNo = referer.GetParm("MenuNo");
            if (menuNo.IsNullOrWhiteSpace() || !view.StartsWith(menuNo)) menuNo = view;
            if ((menuNo ?? "").EndsWith("@"))
            {
                menuNo = menuNo.TrimEnd('@');
                var user = await source.GetCurrentUserAsync();
                if (user?.IsDeveloper == true) return menuNo;
            }
            bool isValid = await new GksybAuthorizeAttribute()
            {
                MenuNo = menuNo,
                IsBaseAuth = isBaseAuth,
                Mode = GksybAuthorizeMode.StartsWith
            }.ValidAsync(source);
            if (!isValid && throwEx) throw new MessageException($"用户无权操作视图{menuNo}");
            return view;
        }
    }
}