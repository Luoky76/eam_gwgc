using Gksyb.Common;
using Gksyb.Common.Mvc.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.Http
{
    public static class IFormFileExtensions
    {
        /// <summary>
        /// 文件另存
        /// </summary>
        /// <returns></returns>
        public static async Task<string> SaveAs(this IFormFile source, string folder = null, string fileName = null, bool isCreateDayDirectory = false)
        {
            var requestServices = Gksyb.Common.Static.HttpContext.RequestServices;
            var webhost = requestServices.GetService<IWebHostEnvironment>();
            var config = requestServices.GetService<IConfiguration>();
            var uploadDirectory = config.GetValue<string>(OptionName.UploadDirectory) ?? nameof(OptionName.UploadDirectory);
            var mapPath = config.GetValue<string>(OptionName.UploadDirectoryMapPath) ?? Path.Combine(webhost.WebRootPath, uploadDirectory);
            folder ??= "";
            folder = new Regex(@"[\\\/\:\*\?\042\<\>\|]").Replace(folder, "");
            var now = DateTime.Now;
            var path = Path.Combine(mapPath, folder, now.ToString("yyyyMM"), isCreateDayDirectory ? now.ToString("dd") : "");
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            if (string.IsNullOrWhiteSpace(fileName)) fileName = $"{GuidHelper.NewShortId()}{Path.GetExtension(source.FileName)}";
            path = Path.Combine(path, fileName);
            var webPath = path.Replace(mapPath, uploadDirectory);
            webPath = webPath.Replace("\\", "/");
            await source.Save(path, webPath);
            return webPath;
        }

        /// <summary>
        /// 文件保存
        /// </summary>
        public static async Task Save(this IFormFile source, string path, string url)
        {
            using var stream = File.Create(path);
            await source.CopyToAsync(stream);
            var service = Gksyb.Common.Static.HttpContext.Current.RequestServices.GetService<IFormFileService>();
            service?.Save(url, path, source);
        }

        /// <summary>
        /// 获取文件hash
        /// </summary>
        public static async Task<string> GetHashAsync(this IFormFile source)
        {
            using var stream = source.OpenReadStream();
            using var hash = MD5.Create();
            return Convert.ToHexString(await hash.ComputeHashAsync(stream));
        }

        /// <summary>
        /// 获取文件编码
        /// </summary>
        public static Encoding GetEncoding(this IFormFile source, Encoding encoding = null)
        {
            using var reader = source.ToStreamReader(encoding);
            return reader.CurrentEncoding ?? encoding;
        }

        /// <summary>
        /// 转成StreamReader
        /// </summary>
        public static StreamReader ToStreamReader(this IFormFile source, Encoding encoding = null)
        {
            encoding ??= Encoding.GetEncoding("GB2312");
            return new StreamReader(source.OpenReadStream(), encoding, true);
        }

        /// <summary>
        /// 安全检查
        /// </summary>
        public static void CheckSecurity(this IFormFile source, ParameterInfo parameterInfo = null)
        {
            if (source == null) return;
            source.CheckSecurity(parameterInfo?.GetCustomAttribute<FileOptionsAttribute>());
        }

        /// <summary>
        /// 安全检查
        /// </summary>
        public static void CheckSecurity(this IFormFile source, FileOptionsAttribute fileOptionsAttribute = null)
        {
            if (source == null) return;
            fileOptionsAttribute?.Valid(source);
            var fileExtension = Path.GetExtension(source.FileName).ToLower();
            if (!_securityContentType.Value.ContainsKey(fileExtension)) throw new MessageException($"不支持的类型{fileExtension}");
            source.CheckImageSecurity();
        }

        /// <summary>
        /// 检查图片安全性
        /// </summary>
        public static void CheckImageSecurity(this IFormFile source)
        {
            var fileExtension = Path.GetExtension(source.FileName).ToLower();
            if (IsImage(fileExtension))
            {
                using var stream = source.OpenReadStream();
                CheckImage(stream).Result();
            }
        }

        /// <summary>
        /// 检查图片安全性
        /// </summary>
        public static string GetContentType(this IFormFile source)
        {
            if (source.Headers != null && !string.IsNullOrWhiteSpace(source.Headers.ContentType)) return source.Headers.ContentType;
            var fileExtension = Path.GetExtension(source.FileName).ToLower();
            if (!_securityContentType.Value.ContainsKey(fileExtension)) return source.ContentType;
            return _securityContentType.Value[fileExtension];
        }

        /// <summary>
        /// 检查是否图片文件
        /// </summary>
        /// <returns></returns>
        public static bool IsImage(string fileExtension)
        {
            if (!_securityContentType.Value.ContainsKey(fileExtension)) return false;
            var contentType = _securityContentType.Value[fileExtension];
            if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断是否真实图片
        /// </summary>
        public static async Task<bool> CheckImage(Stream stream)
        {
            try
            {
                if (stream.CanSeek) stream.Position = 0;
                using Image img = await Image.LoadAsync(stream);
                return true;
            }
            catch (Exception)
            {
                throw new MessageException("不支持的类型");
            }
            finally
            {
                if (stream.CanSeek) stream.Position = 0;
            }
        }

        /// <summary>
        /// 判断是否真实图片
        /// </summary>
        public static bool CheckImage(byte[] content)
        {
            try
            {
                using Image img = Image.Load(content);
                return true;
            }
            catch (Exception)
            {
                throw new MessageException("不支持的类型");
            }
        }

        /// <summary>
        /// 安全文件类型
        /// </summary>
        private static readonly Lazy<Dictionary<string, string>> _securityContentType = new(() =>
        {
            var safeContentType = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { ".jpg", "image/jpeg" },
                { ".png", "image/png" },
                { ".gif", "image/gif" },
                { ".bmp", "image/bmp" },
                { ".ico", "image/x-icon" },
                { ".doc", "application/msword" },
                { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
                { ".csv", "text/csv" },
                { ".xls", "application/vnd.ms-excel" },
                { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },

                { ".avi", "video/x-msvideo" },
                { ".flv", "video/x-flv" },
                { ".jpe", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".md", "text/markdown" },
                { ".mid", "audio/mid" },
                { ".mov", "video/quicktime" },
                { ".mp3", "audio/mpeg" },
                { ".mp4", "video/mp4" },
                { ".mpeg", "video/mpeg" },
                { ".mpg", "video/mpeg" },
                { ".ogg", "video/ogg" },
                { ".ogv", "video/ogg" },
                { ".pdf", "application/pdf" },
                { ".ppt", "application/vnd.ms-powerpoint" },
                { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
                { ".rm", "application/vnd.rn-realmedia" },
                { ".svg", "image/svg+xml" },
                { ".swf", "application/x-shockwave-flash" },
                { ".txt", "text/plain" },
                { ".wav", "audio/wav" },
                { ".webm", "video/webm" },
                { ".webp", "image/webp" },
                { ".wmv", "video/x-ms-wmv" },
                { ".xml", "text/xml" }
            };
            var configContentType = Gksyb.Common.Static.HttpContext.Current.RequestServices.GetService<IConfiguration>().GetSection(OptionName.SafeContentType).Get<Dictionary<string, string>>() ?? new Dictionary<string, string>();
            foreach (var mapping in configContentType)
            {
                if (safeContentType.ContainsKey(mapping.Key)) continue;
                safeContentType.Add(mapping.Key, mapping.Value);
            }
            return safeContentType;
        });
    }
}