using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Text;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace Gksyb.Common
{
    public static class HttpContextExtensions
    {
        private const string UIDName = "GKSYBID";
        private const string TokenName = "GKSYBTOKEN";
        private const string RequestBodyName = "Request.Body";
        private const string ResponseBodyName = "Response.Body";

        /// <summary>
        /// 获取客户端唯一ID
        /// </summary>
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
        public static string GetAuthToken(this HttpContext source, string key = null)
        {
            key ??= TokenName;
            if (source.Request.Headers.TryGetValue(key, out StringValues value))
            {
                if (value == "undefined") return null;
                return value;
            }
            return null;
        }

        /// <summary>
        /// 获取客户端唯一ID
        /// </summary>
        public static string GetClientID(this HttpContext source)
        {
            return source.GetAuthToken() ?? source.GetUID();
        }

        /// <summary>
        /// 获取客户端唯一ID
        /// </summary>
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
        /// 生成JsToken
        /// </summary>
        public static async Task<string> GenerateTokenAsync(this HttpContext source, string key, bool isSkip = false)
        {
            var seed = GuidHelper.NewShortId().GetHashCode();
            var random = new Random(seed);
            var value = Guid.NewGuid().ToString("N")[..random.Next(3, 20)];
            var uid = source.GetClientID();
            key = (key ?? "").TrimStart('/').TrimEnd('/').ToLower();
            key = $"{uid}-{key}";
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
        public static async Task<bool> ValidJsToken(this HttpContext source, string key)
        {
            var uid = source.GetClientID();
            if (string.IsNullOrWhiteSpace(key))
            {
                key = $"{source.Request.PathBase}{source.Request.Path}";
            }
            key = key.TrimStart('/').TrimEnd('/').ToLower();
            key = $"{uid}-{key}";
            var distributedCache = source.RequestServices.GetService<IDistributedCache>();
            var value = (await distributedCache.GetStringAsync(key)) ?? "";
            await distributedCache.RemoveAsync(key);
            return source.Request.Headers[value] == "1";
        }

        /// <summary>
        /// 跨域
        /// </summary>
        public static void CrossDomain(this HttpContext source)
        {
            string url = source.Request.Headers[HeaderNames.Referer];
            url = string.IsNullOrWhiteSpace(url) ? source.Request.GetRealUrl() : url;
            var uri = new Uri(url);
            var domain = uri.AbsoluteUri.Replace(uri.AbsolutePath, "");
            source.Response.CrossDomain(domain);
        }

        public static void SetRequestBodyItem(this HttpContext source, object value)
        {
            if (value == null) return;
            source.Items.Remove(RequestBodyName);
            source.Items.Add(RequestBodyName, value is string ? value : value.ToMiniJson());
        }

        public static string GetRequestBodyItem(this HttpContext source)
        {
            if (source.Items.ContainsKey(RequestBodyName))
            {
                return source.Items[RequestBodyName] as string;
            }
            return source.Request.GetContent().Result();
        }

        public static void SetResponseBodyItem(this HttpContext source, object value)
        {
            if (value == null) return;
            source.Items.Remove(ResponseBodyName);
            source.Items.Add(ResponseBodyName, value.ToMiniJson());
        }

        public static string GetResponseBodyItem(this HttpContext source)
        {
            if (source.Items.ContainsKey(ResponseBodyName))
            {
                return source.Items[ResponseBodyName] as string;
            }
            return source.Response.ContentType;
        }

        /// <summary>
        /// 清空输出并设置状态码
        /// </summary>
        /// <param name="source"></param>
        /// <param name="statusCode">状态码<see cref="StatusCodes"/></param>
        /// <returns></returns>
        public static void ClearWithStatusCode(this HttpResponse source, int statusCode = StatusCodes.Status403Forbidden)
        {
            source.Clear();
            source.StatusCode = statusCode;
            source.Body?.Dispose();
            source.Body = Stream.Null;
        }
    }
}