using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

namespace Gksyb.Common
{
    public static class HttpResponseExtensions
    {
        static HttpResponseExtensions()
        {
            var configuration = Static.HttpContext.RequestServices.GetService<IConfiguration>();
            Domain = configuration.GetValue<string>("Security:Domain");
            Headers = configuration.GetSection("Kestrel:Headers").Get<Dictionary<string, string>>();
        }

        /// <summary>
        /// 添加跨域头
        /// </summary>
        /// <param name="source">HttpResponse</param>
        /// <param name="domain">域名</param>
        public static void CrossDomain(this HttpResponse source, string domain = "")
        {
            try
            {
                source.Headers[HeaderNames.AccessControlAllowOrigin] = domain;
                source.Headers[HeaderNames.AccessControlAllowCredentials] = "true";
            }
            catch
            {
            }
        }

        /// <summary>
        /// 添加安全响应头
        /// </summary>
        /// <param name="source"></param>
        public static void AddSecurityHeader(this HttpResponse source)
        {
            try
            {
                var domain = Domain;
                //安全响应头
                source.Headers[HeaderNames.ContentSecurityPolicy] = $"manifest-src 'self';frame-ancestors 'self' {domain};";
                source.Headers[HeaderNames.StrictTransportSecurity] = "max-age=31536000";
                source.Headers[HeaderNames.XFrameOptions] = string.IsNullOrWhiteSpace(domain) ? "SAMEORIGIN" : $"ALLOW-FROM {domain}";
                source.Headers[HeaderNames.XContentTypeOptions] = "nosniff";
                source.Headers[HeaderNames.XXSSProtection] = "1";
                source.Headers["X-Download-Options"] = "noopen";
                source.Headers["X-Permitted-Cross-Domain-Policies"] = "master-only";
                source.Headers["Referrer-Policy"] = "no-referrer-when-downgrade";
                var headers = Headers;
                if (headers != null)
                {
                    foreach (var header in Headers)
                    {
                        source.Headers[header.Key] = header.Value;
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 安全域名
        /// </summary>
        public static readonly string Domain;

        /// <summary>
        /// 追加的Headers头
        /// </summary>
        private static readonly Dictionary<string, string> Headers;
    }
}