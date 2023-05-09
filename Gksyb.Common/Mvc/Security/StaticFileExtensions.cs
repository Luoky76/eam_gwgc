using Gksyb.Common;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class StaticFileExtensions
    {
        /// <summary>
        /// 安全响应头
        /// </summary>
        /// <returns></returns>
        public static IApplicationBuilder UseSafeStaticFiles(this IApplicationBuilder app, StaticFileOptions options = null, Action<StaticFileResponseContext> action = null)
        {
            options ??= new StaticFileOptions();
            options.ContentTypeProvider ??= new WebFileContentTypeProvider();
            options.OnPrepareResponse = ctx =>
            {
                var request = ctx.Context.Request;
                var response = ctx.Context.Response;
                if (!request.IsInnerRequest(Domin))//防盗链
                {
                    response.ClearWithStatusCode();
                    return;
                }
                action?.Invoke(ctx);
            };
            return app.UseStaticFiles(options);
        }

        private static string _domin = null;

        /// <summary>
        /// 安全域名
        /// </summary>
        private static string Domin
        {
            get
            {
                if (_domin != null) return _domin;
                var configuration = Gksyb.Common.Static.HttpContext.RequestServices.GetService<IConfiguration>();
                _domin = configuration.GetValue<string>("Security:Domin");
                return _domin;
            }
        }
    }
}