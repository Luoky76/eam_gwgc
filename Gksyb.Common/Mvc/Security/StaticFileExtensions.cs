using Gksyb.Common;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Builder
{
    public static class StaticFileExtensions
    {
        /// <summary>
        /// 安全响应头
        /// </summary>
        /// <returns></returns>
        public static IApplicationBuilder UseSafeStaticFiles(this IApplicationBuilder app, StaticFileOptions options = null, Action<StaticFileResponseContext> action = null, bool noCache = true)
        {
            options ??= new StaticFileOptions();
            options.ContentTypeProvider ??= new WebFileContentTypeProvider();
            options.OnPrepareResponse = ctx =>
            {
                var request = ctx.Context.Request;
                var response = ctx.Context.Response;
                if (!request.IsInnerRequest(HttpResponseExtensions.Domain))//防盗链
                {
                    response.ClearWithStatusCode();
                    return;
                }
                action?.Invoke(ctx);
                if (!noCache) return;
                var noHtml = response.ContentType != "text/html";
                if (noHtml && response.ContentType != "text/javascript") return;
                var cache = request.QueryString.HasValue ? request.QueryString.Value.GetParm("cache") : "";
                if (cache == "1") return;
                if (cache != "0" && noHtml && ((response.ContentLength ?? long.MaxValue) > _cacheSize || request.Path.StartsWithSegments("/lib"))) return;
                response.Headers[HeaderNames.CacheControl] = _cacheControl;
            };
            return app.UseStaticFiles(options);
        }

        /// <summary>
        /// 不缓存
        /// </summary>
        private const string _cacheControl = "no-cache, no-store, must-revalidate, max-age=0";

        /// <summary>
        /// 最小不缓存大小
        /// </summary>
        private const long _cacheSize = 100 * 1024;
    }
}