using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using System.IO;

namespace Gksyb.Common
{
    public static class HttpContextExtensions
    {
        private static readonly string RequestBodyName = "Request.Body";

        private static readonly string ResponseBodyName = "Response.Body";

        /// <summary>
        /// 跨域
        /// </summary>
        /// <param name="source">HttpContext</param>
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
            source.Items.Add(RequestBodyName, value.ToMiniJson());
        }

        public static string GetRequestBodyItem(this HttpContext source)
        {
            var body = source.Request.ContentType;
            if (source.Items.ContainsKey(RequestBodyName))
            {
                body = source.Items[RequestBodyName] as string;
            }
            return body;
        }

        public static void SetResponseBodyItem(this HttpContext source, object value)
        {
            if (value == null) return;
            source.Items.Remove(ResponseBodyName);
            source.Items.Add(ResponseBodyName, value.ToMiniJson());
        }

        public static string GetResponseBodyItem(this HttpContext source)
        {
            var body = source.Response.ContentType;
            if (source.Items.ContainsKey(ResponseBodyName))
            {
                body = source.Items[ResponseBodyName] as string;
            }
            return body;
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