namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// 文件属性
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    public class FileOptionsAttribute : Attribute
    {
        /// <summary>
        /// 允许上传的文件类型逗号分隔
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// 最大上传文件大小（单位M）
        /// </summary>
        public long FileMaxLength { get; set; }

        /// <summary>
        /// 文件属性
        /// </summary>
        public FileOptionsAttribute()
        {
        }

        /// <summary>
        /// 文件属性
        /// </summary>
        public FileOptionsAttribute(string fileType, long fileMaxLength = 2)
        {
            FileType = fileType;
            FileMaxLength = fileMaxLength;
        }

        public void Valid(IFormFile formFile)
        {
            if (FileMaxLength <= 0) return;
            if (formFile.Length > FileMaxLength * 1024 * 1024) throw new MessageException($"最大只能上传{FileMaxLength}M文件");
            if (string.IsNullOrWhiteSpace(FileType)) return;
            var fileExtension = Path.GetExtension(formFile.FileName) ?? "";
            if (!FileType.Split(",").Any(filtType => fileExtension.Equals($".{filtType}", StringComparison.OrdinalIgnoreCase)))
                throw new MessageException($"只支持的类型{FileType}");
        }
    }
}