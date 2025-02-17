using Gksyb.Common;
using Gksyb.Common.Mvc.Dtos;
using Gksyb.Common.Mvc.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SkiaSharp;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Microsoft.AspNetCore.Http
{
    public static class IFormFileExtensions
    {
        /// <summary>
        /// 不需要权限访问的公共文件夹
        /// </summary>
        public const string Public = "public";

        /// <summary>
        /// 验证用户
        /// </summary>
        public const string Auth = "auth";

        /// <summary>
        /// 文件压缩
        /// </summary>
        /// <param name="source">上传文件</param>
        /// <param name="quality">图片质量</param>
        /// <param name="width">图片宽度</param>
        /// <param name="height">图片高度</param>
        public static async Task<IFormFile> CompressAsync(this IFormFile source, int quality = 90, int width = 800, int height = 0)
        {
            using var inputStream = new MemoryStream();
            await source.CopyToAsync(inputStream);
            var outputStream = Compress(inputStream, quality, width, height);
            // 创建 IFormFile 对象
            var formFile = new FormFile(outputStream, 0, outputStream.Length, source.Name, source.FileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
            return formFile;
        }

        /// <summary>
        /// 文件压缩
        /// </summary>
        /// <param name="source">上传文件</param>
        /// <param name="quality">图片质量</param>
        /// <param name="width">图片宽度</param>
        /// <param name="height">图片高度</param>
        public static Stream Compress(this Stream source, int quality = 90, int width = 800, int height = 0)
        {
            if (source.CanSeek) source.Seek(0, SeekOrigin.Begin);
            using var original = SKBitmap.Decode(source);

            var scale = 1.0;
            if (width > 0)
            {
                scale = width * 1.0 / original.Width;
            }
            else if (height > 0)
            {
                scale = height * 1.0 / original.Height;
            }
            width = (int)(original.Width * scale);
            height = (int)(original.Height * scale);

            using var resized = original.Resize(new SKImageInfo(width, height), SKFilterQuality.High);
            using var image = SKImage.FromBitmap(resized);
            var outputStream = new MemoryStream();
            // 将压缩后的图片保存为 JPEG 格式
            image.Encode(SKEncodedImageFormat.Jpeg, quality).SaveTo(outputStream);
            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream;
        }

        /// <summary>
        /// 文件另存
        /// </summary>
        /// <param name="source">上传文件</param>
        /// <param name="folder">主文件夹</param>
        /// <param name="fileName">文件名称（自定义文件名称后可忽略hash比对）</param>
        /// <param name="isCreateDayDirectory">文件按天分文件夹</param>
        /// <returns></returns>
        public static async Task<string> SaveAs(this IFormFile source, string folder = null, string fileName = null, bool isCreateDayDirectory = false)
        {
            folder ??= "";
            folder = new Regex(@"[\\\/\:\*\?\042\<\>\|]").Replace(folder, "");
            var now = DateTime.Now;
            var path = Path.Combine(folder, now.ToString("yyyyMM"), isCreateDayDirectory ? now.ToString("dd") : "");
            var emptyName = string.IsNullOrWhiteSpace(fileName);
            if (emptyName) fileName = $"{GuidHelper.NewShortId()}_{source.FileName[Math.Max(0, source.FileName.Length - 120)..]}";
            return await Save(source, Path.Combine(path, fileName), !emptyName);
        }

        /// <summary>
        /// 文件保存
        /// </summary>
        /// <param name="source">上传文件</param>
        /// <param name="path">文件相对路径</param>
        /// <param name="ignoreHash">忽略hash比对</param>
        /// <returns></returns>
        public static async Task<string> Save(IFormFile source, string path, bool ignoreHash = false)
        {
            var mapPath = MapPath(out var uploadDirectory);
            var allPath = Path.GetFullPath(Path.Combine(mapPath, path.TrimStart(Path.DirectorySeparatorChar)));
            MessageException.ThrowIf(!allPath.StartsWith(mapPath), "路径错误");
            path = Path.GetRelativePath(mapPath, allPath);
            var directory = Path.GetDirectoryName(allPath);
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            var url = $"{uploadDirectory}/{path.Replace(Path.DirectorySeparatorChar, '/')}";
            url = HttpUtility.UrlPathEncode(url);
            var service = Gksyb.Common.Static.HttpContext.Current.RequestServices.GetService<IFormFileService>();
            var fileRequest = new FormFileRequest()
            {
                Url = url,
                Path = path,
                MapPath = mapPath,
                FormFile = source,
                IgnoreHash = ignoreHash
            };
            var saveUrl = await service?.SaveAsync(fileRequest);
            if (saveUrl != url) return saveUrl;
            using var stream = File.Create(allPath);
            await source.CopyToAsync(stream);
            return url;
        }

        /// <summary>
        /// map路径
        /// </summary>
        public static string MapPath()
        {
            return MapPath(out var _);
        }

        /// <summary>
        /// map路径
        /// </summary>
        public static string MapPath(out string uploadDirectory)
        {
            var requestServices = Gksyb.Common.Static.HttpContext.RequestServices;
            var webhost = requestServices.GetService<IWebHostEnvironment>();
            var config = requestServices.GetService<IConfiguration>();
            uploadDirectory = config.GetValue<string>(OptionName.UploadDirectory) ?? nameof(OptionName.UploadDirectory);
            var mapPath = config.GetValue<string>(OptionName.UploadDirectoryMapPath) ?? Path.Combine(webhost.WebRootPath, uploadDirectory);
            return mapPath;
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
                CheckImage(stream);
            }
            if (fileExtension == ".pdf")//pdf由于浏览器可以直接打开，需要处理xss
            {
                CheckPdf(source);
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
            if (contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) && contentType != "image/svg+xml")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断是否真实图片
        /// </summary>
        public static bool CheckImage(Stream stream)
        {
            try
            {
                if (stream.CanSeek) stream.Position = 0;
                SKBitmap img = SKBitmap.Decode(stream);
                MessageException.ThrowIf(img == null, "不支持的类型");
                return true;
            }
            catch (Exception)
            {
                throw new MessageException("不支持的类型");
            }
        }

        /// <summary>
        /// 判断是否真实图片
        /// </summary>
        public static bool CheckImage(byte[] content)
        {
            try
            {
                using SKBitmap img = SKBitmap.Decode(content);
                MessageException.ThrowIf(img == null, "不支持的类型");
                return true;
            }
            catch (Exception)
            {
                throw new MessageException("不支持的类型");
            }
        }

        /// <summary>
        /// 判断PDF是否安全
        /// </summary>
        public static void CheckPdf(IFormFile file)
        {
            using var stream = new MemoryStream();
            file.CopyTo(stream);
            if (stream.CanSeek) stream.Position = 0;
            using var reader = new StreamReader(stream, Encoding.UTF8);
            var content = reader.ReadToEnd();
            if (Regex.IsMatch(content, @"\bjavascript\b|\balert\b", RegexOptions.IgnoreCase))
                throw new MessageException("防注入:1003");
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