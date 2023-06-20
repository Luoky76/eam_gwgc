using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNetCore.StaticFiles
{
    /// <summary>
    /// 文件内容类型服务
    /// </summary>
    public class WebFileContentTypeProvider : IContentTypeProvider
    {
        public WebFileContentTypeProvider(IDictionary<string, string> mappings = null)
        {
            if (mappings == null) return;
            foreach (var mapping in mappings)
            {
                if (Mappings.ContainsKey(mapping.Key)) continue;
                Mappings.Add(mapping.Key, mapping.Value);
            }
        }

        /// <summary>
        /// 获取contentType
        /// </summary>
        /// <returns></returns>
        public bool TryGetContentType(string subpath, [MaybeNullWhen(false)] out string contentType)
        {
            var extension = GetExtension(subpath);
            if (extension != null) return Mappings.TryGetValue(extension, out contentType);
            contentType = null;
            return false;
        }

        private static string GetExtension(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            int index = path.LastIndexOf('.');
            if (index < 0) return null;
            return path[index..];
        }

        private static readonly Dictionary<string, string> Mappings = new(StringComparer.OrdinalIgnoreCase)
        {
            { ".html", "text/html" },
            { ".htm", "text/html" },
            { ".js", "text/javascript" },
            { ".css", "text/css" },
            { ".ico", "image/x-icon" },
            { ".jpg", "image/jpeg" },
            { ".png", "image/png" },
            { ".gif", "image/gif" },
            { ".bmp", "image/bmp" },

            { ".apk", "application/vnd.android.package-archive" },
            { ".avi", "video/x-msvideo" },
            { ".csv", "text/csv" },
            { ".doc", "application/msword" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".eot", "application/vnd.ms-fontobject" },
            { ".flv", "video/x-flv" },
            { ".jpe", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".json", "application/json" },
            { ".md", "text/markdown" },
            { ".mid", "audio/mid" },
            { ".mov", "video/quicktime" },
            { ".mp3", "audio/mpeg" },
            { ".mp4", "video/mp4" },
            { ".mpeg", "video/mpeg" },
            { ".mpg", "video/mpeg" },
            { ".ogg", "video/ogg" },
            { ".ogv", "video/ogg" },
            { ".otf", "font/otf" },
            { ".pdf", "application/pdf" },
            { ".ppt", "application/vnd.ms-powerpoint" },
            { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
            { ".rm", "application/vnd.rn-realmedia" },
            { ".svg", "image/svg+xml" },
            { ".swf", "application/x-shockwave-flash" },
            { ".txt", "text/plain" },
            { ".ttf", "application/x-font-ttf" },
            { ".wav", "audio/wav" },
            { ".webm", "video/webm" },
            { ".webp", "image/webp" },
            { ".wmv", "video/x-ms-wmv" },
            { ".woff", "application/font-woff" },
            { ".woff2", "font/woff2" },
            { ".xls", "application/vnd.ms-excel" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".xml", "text/xml" }
        };
    }
}