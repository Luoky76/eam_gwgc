using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Gksyb.Common
{
    public static class HttpRequestExtensions
    {
        static HttpRequestExtensions()
        {
            var configuration = Static.HttpContext.RequestServices.GetService<IConfiguration>();
            XForwardedFor = configuration.GetValue<string>("Security:XForwardedFor");
            if (string.IsNullOrWhiteSpace(XForwardedFor))
            {
                var cacheOptions = configuration.GetSection(OptionName.RedisCache).Get<RedisCacheOptions>();
                if (!string.IsNullOrWhiteSpace(cacheOptions.Configuration))
                {
                    XForwardedFor = "X-Real-IP";
                }
            }
        }

        /// <summary>
        /// 获取URI参数
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parm"></param>
        public static string GetParm(this string url, string parm)
        {
            var match = Regex.Match(url, string.Format(@"(?![?&])({0})=([^?&]*)", parm), RegexOptions.IgnoreCase);
            if (!match.Success) return "";
            return HttpUtility.UrlDecode(match.Value.Split("=")[1].Trim());
        }

        /// <summary>
        /// 获取URI参数
        /// </summary>
        /// <param name="source">uri</param>
        /// <param name="parm">参数名</param>
        /// <returns></returns>
        public static string GetParm(this Uri source, string parm) => source == null ? "" : source.Query.GetParm(parm);

        /// <summary>
        /// 移除URL参数
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public static string RemoveUrlParam(this string url, string[] parms)
        {
            foreach (var parm in parms)
            {
                url = Regex.Replace(url, string.Format(@"(?![?&])({0})=([^?&]*)", parm), "", RegexOptions.IgnoreCase);
            }
            url = url.TrimEnd('&').TrimEnd('?');
            return url;
        }

        /// <summary>
        /// 移除URL参数
        /// </summary>
        /// <param name="source"></param>
        /// <param name="parms"></param>
        public static string RemoveUrlParam(this HttpRequest source, string[] parms)
        {
            var url = source.GetEncodedPathAndQuery();
            return url.RemoveUrlParam(parms);
        }

        /// <summary>
        /// 是否Ajax请求
        /// </summary>
        /// <param name="source"></param>
        public static bool IsAjax(this HttpRequest source) => "XMLHttpRequest".Equals(source.Headers[HeaderNames.XRequestedWith], StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// 获取真实IP
        /// </summary>
        /// <param name="source"></param>
        /// <param name="hasPort"></param>
        /// <returns></returns>
        public static string GetRealIP(this HttpRequest source, bool hasPort = false)
        {
            var ip = string.IsNullOrWhiteSpace(XForwardedFor) ? string.Empty : source.Headers[XForwardedFor].ToString();
            if (!ip.Contains('.')) ip = null;
            if (string.IsNullOrWhiteSpace(ip))
            {
                ip = source.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
                if (ip == "0.0.0.1") ip = "127.0.0.1";
                if (hasPort) ip = $"{ip}:{source.HttpContext.Connection.RemotePort}";
                return ip;
            }
            var ips = ip.Split(',').Select(c => c.Split(":")[0].Trim()).ToList();
            if (ips.Count == 1) return ips[0];
            ip = ips.Where(c => !IsInnerIP(c)).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(ip)) return ip;
            ip = ips.Where(c => c.IsIpAddress()).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(ip)) return ip;
            return ips[0];
        }

        /// <summary>
        /// 获取用户代理
        /// </summary>
        public static string GetUserAgent(this HttpRequest source) => source.Headers[HeaderNames.UserAgent];

        /// <summary>
        /// 获取真实地址
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetRealUrl(this HttpRequest source)
        {
            var host = source.GetRealHost();
            var url = source.GetEncodedUrl().Replace(source.Host.Value, host).Replace(":80/", "/").Replace(":443/", "/");
            if (source.IsHttps)
            {
                url = Regex.Replace(url, "http://", "https://", RegexOptions.IgnoreCase);
            }
            return url;
        }

        /// <summary>
        /// 获取真实host
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string GetRealHost(this HttpRequest source) => source.Host.Value;

        /// <summary>
        /// 判断是否内部IP
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsInnerIP(this HttpRequest source) => IsInnerIP(source.GetRealIP());

        /// <summary>
        /// 判断是否内部IP
        /// </summary>
        /// <param name="ipAddress">判断是否内部IP</param>
        /// <returns></returns>
        public static bool IsInnerIP(string ipAddress)
        {
            if (ipAddress == "::1") return true;
            if (ipAddress == "127.0.0.1") return true;
            if (!ipAddress.IsIpAddress()) return false;
            var byteArray = ipAddress.Split('.').Select(c => byte.Parse(c)).ToArray();
            var b0 = byteArray[0];
            var b1 = byteArray[1];
            var b2 = byteArray[2];
            //A类 10.0.0.0-10.255.255.255
            const byte SECTION_1 = 10;
            //B类 172.16.0.0-172.31.255.255
            const byte SECTION_2 = 172;
            const byte SECTION_3 = 16;
            const byte SECTION_4 = 31;
            //C类 192.168.0.0-192.168.255.255
            const byte SECTION_5 = 192;
            const byte SECTION_6 = 168;
            //港口事业部 192.101.109.*
            const byte SECTION_7 = 101;
            const byte SECTION_8 = 109;
            switch (b0)
            {
                case SECTION_1:
                    return true;

                case SECTION_2:
                    if (b1 >= SECTION_3 && b1 <= SECTION_4)
                    {
                        return true;
                    }
                    break;

                case SECTION_5:
                    switch (b1)
                    {
                        case SECTION_6:
                            return true;

                        case SECTION_7:
                            if (b2 == SECTION_8) return true;
                            return false;
                    }
                    break;

                default:
                    return false;
            }
            return false;
        }

        /// <summary>
        /// 请求是否来本网站或内网请求
        /// </summary>
        /// <param name="source"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        public static bool IsInnerRequest(this HttpRequest source, string domain = "")
        {
            if (!source.Headers.ContainsKey(HeaderNames.Referer)) return true;
            var uri = new Uri(source.Headers[HeaderNames.Referer]);
            var host = source.GetRealHost().Replace(":80", "").Replace(":443", "");
            var uriHost = uri.Authority.Replace(":80", "").Replace(":443", "");
            if (uriHost == host) return true;
            if (!string.IsNullOrEmpty(domain))
            {
                if (domain.Split(",").Any(c => c.EndsWith(uriHost)))
                {
                    return true;
                }
            }
            return source.IsInnerIP();
        }

        /// <summary>
        /// 判断是否在微信内置浏览器中
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static bool IsWeixinBrowser(this HttpRequest source)
        {
            string userAgent = source.Headers[HeaderNames.UserAgent];
            if (userAgent != null && (userAgent.Contains("MicroMessenger")))
            {
                if (source.Query.ContainsKey("SkipWX"))
                {
                    return false;//不是微信公众号，但可以让微信预览
                }
                return true;//在微信内部
            }
            else
            {
                return false;//在微信外部
            }
        }

        /// <summary>
        /// 获取request内容体
        /// </summary>
        internal static async Task<string> GetBodyAsync(this HttpRequest source)
        {
            var json = string.Empty;
            try
            {
                source.EnableBuffering();
                if (!source.Body.CanSeek)
                    return json;
                source.Body.Position = 0;
                using var ms = new MemoryStream();
                await source.Body.CopyToAsync(ms);
                ms.Position = 0;
                using var reader = new StreamReader(ms, source.GetEncoding());
                json = await reader.ReadToEndAsync();
                source.Body.Position = 0;
            }
            catch (BadHttpRequestException ex)
            {
                var logger = source.HttpContext.RequestServices.GetRequiredService<ILogger<HttpRequest>>();
                logger.LogError(new LogPath("Exception"), $"{source.ContentType} {source.ContentLength} {ex}");
            }
            return json;
        }

        /// <summary>
        /// 获取编码
        /// </summary>
        /// <param name="source"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static Encoding GetEncoding(this HttpRequest source, Encoding encoding = null)
        {
            var contentType = source.ContentType ?? "";
            encoding ??= Encoding.UTF8;
            var match = Regex.Match(contentType, string.Format(@"({0})=([^?&]*)", "charset"), RegexOptions.IgnoreCase);
            if (match.Success)
            {
                try
                {
                    var charset = match.Value.Split("=")[1].Trim();
                    return Encoding.GetEncoding(charset) ?? encoding;
                }
                catch
                {
                }
            }
            return encoding;
        }

        /// <summary>
        /// 获取变量
        /// </summary>
        /// <returns></returns>
        public static string GetParm(this HttpRequest source, string parm)
        {
            if (source.Query.ContainsKey(parm)) return source.Query[parm];
            if (source.HasFormContentType && source.Form.ContainsKey(parm)) return source.Form[parm];
            if (source.Headers.ContainsKey(parm)) return source.Headers[parm];
            return null;
        }

        /// <summary>
        /// 允许重复读取body
        /// </summary>
        public static void EnableRewind(this HttpRequest source, long maxLength = 10 * 1024)
        {
            if (!source.HasFormContentType) return;
            if (source.ContentLength > maxLength) return;
            if (source.Body?.CanSeek == true) return;
            if (source.ContentType.StartsWith("multipart/form-data", StringComparison.OrdinalIgnoreCase)) return;
            source.EnableBuffering();
        }

        /// <summary>
        /// XFF头
        /// </summary>
        private static readonly string XForwardedFor;
    }
}