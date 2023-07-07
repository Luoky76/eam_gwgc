using Gksyb.Core.Auth;
using Gksyb.Model.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Microsoft.AspNetCore.Builder
{
    public static class UploadDirectoryExtensions
    {
        private static string Passport;
        private static string MapPath;

        /// <summary>
        /// 上传目录安全处理
        /// </summary>
        public static IApplicationBuilder UseUploadDirectory(this IApplicationBuilder app)
        {
            var requestServices = Gksyb.Common.Static.HttpContext.RequestServices;
            var configuration = requestServices.GetService<IConfiguration>();
            var webhost = requestServices.GetService<IWebHostEnvironment>();
            var path = configuration.GetValue<string>(OptionName.UploadDirectory) ?? nameof(OptionName.UploadDirectory);
            MapPath = configuration.GetValue<string>(OptionName.UploadDirectoryMapPath) ?? Path.Combine(webhost.WebRootPath, path);
            if (!Directory.Exists(MapPath)) return app;
            Passport = configuration.GetValue($"{OptionName.SysContext}:Passport", "t6VJoFPZwq7jGpyHi20ucv3eaW4NAz9IdkmLEX5Csr!fQKOSbD#hTBxU8R@Yl1gn");
            var staticFileOptions = new StaticFileOptions()
            {
                RequestPath = new PathString($"/{path}"),
                FileProvider = new PhysicalFileProvider(MapPath)
            };
            app.UseSafeStaticFiles(staticFileOptions, ctx =>
            {
                if (ctx.File.PhysicalPath.Contains($"{IFormFileExtensions.Public}\\", StringComparison.OrdinalIgnoreCase)) return;//带有Public的文件夹不验证权限
                if (ctx.Context.Request.Headers["Passport"] == Passport) return;//有通行证的不验证权限
                if (Valid(ctx.Context, ctx.File)) return;
                ctx.Context.Response.ClearWithStatusCode();
            });
            return app;
        }

        /// <summary>
        /// 验证是否有访问上传文件的权限
        /// </summary>
        /// <param name="context"></param>
        /// <param name="fileInfo"></param>
        /// <returns></returns>
        private static bool Valid(HttpContext context, IFileInfo fileInfo)
        {
            var token = context.GetUID(false);
            if (string.IsNullOrWhiteSpace(token)) return false;
            var user = context.GetCurrentUserAsync(token).Result();
            if (user == null) return false;
            var directory = Path.GetDirectoryName(fileInfo.PhysicalPath.Replace(MapPath, "")).Trim(Path.DirectorySeparatorChar);
            if (!$"{directory}{Path.DirectorySeparatorChar}".Contains($"{IFormFileExtensions.Auth}{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase)) return true;
            var dbContext = context.RequestServices.GetService<IDbContext>();
            return dbContext.Query<SYS_FILE>().Where(c => c.FILE_PATH == directory && c.CREATEUSERID == user.UserID).Any();
        }
    }
}