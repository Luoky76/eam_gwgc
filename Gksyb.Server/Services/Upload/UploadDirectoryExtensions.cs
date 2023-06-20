using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Model.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.IO;

namespace Microsoft.AspNetCore.Builder
{
    public static class UploadDirectoryExtensions
    {
        /// <summary>
        /// 上传目录安全处理
        /// </summary>
        public static IApplicationBuilder UseUploadDirectory(this IApplicationBuilder app)
        {
            var requestServices = Gksyb.Common.Static.HttpContext.RequestServices;
            var configuration = requestServices.GetService<IConfiguration>();
            var webhost = requestServices.GetService<IWebHostEnvironment>();
            var path = configuration.GetValue<string>(OptionName.UploadDirectory) ?? nameof(OptionName.UploadDirectory);
            var mapPath = configuration.GetValue<string>(OptionName.UploadDirectoryMapPath) ?? Path.Combine(webhost.WebRootPath, path);
            if (!Directory.Exists(mapPath)) return app;
            var staticFileOptions = new StaticFileOptions()
            {
                RequestPath = new PathString($"/{path}"),
                FileProvider = new PhysicalFileProvider(mapPath)
            };
            app.UseSafeStaticFiles(staticFileOptions, ctx =>
            {
                if (ctx.File.PhysicalPath.Contains("Public\\", StringComparison.OrdinalIgnoreCase)) return;//带有Public的文件夹不验证权限
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
            if (!fileInfo.PhysicalPath.Contains("Auth\\", StringComparison.OrdinalIgnoreCase)) return true;
            var dbContext = context.RequestServices.GetService<IDbContext>();
            var fileUser = dbContext.Query<SYS_FILE>().Where(c => c.FILE_PATH == fileInfo.PhysicalPath).Select(c => new UserInfo()
            {
                Id = c.CREATEUSERID,
                Name = c.CREATEUSER
            }).FirstOrDefault();
            if (user.UserID == fileUser?.Id) return true;
            return false;
        }
    }
}