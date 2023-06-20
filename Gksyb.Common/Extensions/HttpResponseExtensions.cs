using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Gksyb.Common
{
    public static class HttpResponseExtensions
    {
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
                //安全响应头
                source.Headers[HeaderNames.ContentSecurityPolicy] = "manifest-src 'self'";
                source.Headers[HeaderNames.StrictTransportSecurity] = "max-age=31536000";
                source.Headers[HeaderNames.XFrameOptions] = "SAMEORIGIN";
                source.Headers["X-Content-Type-Options"] = "nosniff";
                source.Headers["X-XSS-Protection"] = "1";
                source.Headers["X-Download-Options"] = "noopen";
                source.Headers["X-Permitted-Cross-Domain-Policies"] = "master-only";
                source.Headers["Referrer-Policy"] = "no-referrer-when-downgrade";
            }
            catch
            {
            }
        }
    }
}