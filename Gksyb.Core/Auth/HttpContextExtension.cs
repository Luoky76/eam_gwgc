using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Gksyb.Core.Auth
{
    public static class HttpContextExtension
    {
        private static readonly string UIDName = "GKSYBID";
        private static readonly string TokenName = "GKSYBTOKEN";

        /// <summary>
        /// 获取客户端唯一ID
        /// </summary>
        /// <returns></returns>
        public static string GetUID(this HttpContext source, bool isBuild = true)
        {
            if (!source.Request.Cookies.TryGetValue(UIDName, out string uid))
            {
                if (!isBuild) return uid;
                uid = GuidHelper.NewShortId();
                source.SetClientID(uid);
            }
            return uid;
        }

        /// <summary>
        /// 获取Token的值
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetAuthToken(this HttpContext source)
        {
            if (source.Request.Headers.TryGetValue(TokenName, out StringValues value))
            {
                if (value == "undefined") return null;
                return value;
            }
            return null;
        }

        /// <summary>
        /// 获取客户端唯一ID
        /// </summary>
        /// <returns></returns>
        public static string GetClientID(this HttpContext source)
        {
            return source.GetAuthToken() ?? source.GetUID();
        }

        /// <summary>
        /// 获取客户端唯一ID
        /// </summary>
        /// <returns></returns>
        public static void SetClientID(this HttpContext source, string id)
        {
            source.Response.Cookies.Append(UIDName, id, new CookieOptions()
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = source.Request.IsHttps
                //Expires = DateTime.MaxValue
            });
        }

        /// <summary>
        /// 根据token获取用户会话信息
        /// </summary>
        public static async Task<UserSession> GetCurrentUserAsync(this HttpContext source, string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;
            var distributedCache = source.RequestServices.GetService<IDistributedCache>();
            var userSession = await distributedCache.GetAsync<UserSession>(token, default);
            if (userSession == null) return userSession;
            if (!userSession.CheckIP(source.Request.GetRealIP(), distributedCache)) return null;
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
            userSession = await source.GetCurrentUserAsync(token);
            if (userSession == null) return userSession;
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
            var ip = source == null ? Gksyb.Common.Static.HttpContext.AddressList.ToStr(",") : source.Request?.GetRealIP();
            return new UserSession()
            {
                RealName = ip,
                IP = ip
            };
        }

        /// <summary>
        /// 生成JsToken
        /// </summary>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task<string> GenerateTokenAsync(this HttpContext source, string key, bool isSkip = false)
        {
            var seed = GuidHelper.NewShortId().GetHashCode();
            var random = new Random(seed);
            var value = Guid.NewGuid().ToString("N")[..random.Next(3, 20)];
            var uid = source.GetClientID();
            key = $"{uid}-{key}";
            key = key.ToLower();
            var distributedCache = source.RequestServices.GetService<IDistributedCache>();
            await distributedCache.SetStringAsync(key, value, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
            });
            if (isSkip) return value;
            var fn = ((char)(random.Next(65, 90))).ToString();
            var an = ((char)(random.Next(97, 122))).ToString();
            var rn = random.Next(0, value.Length - 1);
            var pn = random.Next(0, 10);
            var result = new StringBuilder();
            result.Append($" var {fn.PadLeft(pn, ' ')}=jqXHR.setRequestHeader;var {an.PadRight(pn, ' ')}=[];");
            for (int i = rn, l = value.Length; i < l; i++)
            {
                result.Append($"{an.PadLeft(pn, ' ')}[{i}] = \"{value[i].ToString().ToUnicodeString()}\";");
            }
            for (int i = rn - 1; i >= 0; i--)
            {
                result.Append($"{an.PadLeft(random.Next(0, 10), ' ')}[{i}] = \"{value[i].ToString().ToUnicodeString()}\";");
            }
            result.Append($"{fn}.call(jqXHR,{an.PadLeft(pn, ' ')}.join(''), \"1\");{fn}=\"{"".ToUnicodeString()}\";");
            return result.ToString();
        }

        /// <summary>
        /// 验证JsToken
        /// </summary>
        /// <param name="source"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task<bool> ValidJsToken(this HttpContext source, string key)
        {
            var uid = source.GetClientID();
            key = $"{uid}-{key}";
            key = key.ToLower();
            var distributedCache = source.RequestServices.GetService<IDistributedCache>();
            var value = (await distributedCache.GetStringAsync(key)) ?? "";
            await distributedCache.RemoveAsync(key);
            return source.Request.Headers[value] == "1";
        }

        /// <summary>
        /// 验证视图
        /// </summary>
        /// <returns></returns>
        public static async Task<bool> ValidViewAsync(this HttpContext source, string view, bool throwEx = true)
        {
            view ??= "";
            if (view.EndsWith("Public")) return true;
            var isBaseAuth = false;
            if (view.EndsWith("Common")) isBaseAuth = true;
            var request = source?.Request;
            if (request == null) return true;
            var menuNo = request.GetRealUrl().GetParm("MenuNo");
            var referer = request.Headers[HeaderNames.Referer].ToString();
            if (menuNo.IsNullOrWhiteSpace()) menuNo = referer.GetParm("MenuNo");
            if (menuNo.IsNullOrWhiteSpace() || !view.StartsWith(menuNo)) menuNo = view;
            bool isValid = await new GksybAuthorizeAttribute()
            {
                MenuNo = menuNo,
                IsBaseAuth = isBaseAuth,
                IsStartsWith = true
            }.ValidAsync(source);
            if (!isValid && throwEx) throw new MessageException($"用户无权操作视图{menuNo}");
            return isValid;
        }
    }
}