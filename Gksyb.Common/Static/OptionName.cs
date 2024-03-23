namespace Microsoft.Extensions.Configuration
{
    public class OptionName
    {
        /// <summary>
        /// 数据库配置
        /// </summary>
        public const string DataBase = "DataBase";

        /// <summary>
        /// 系统配置
        /// </summary>
        public const string SysContext = "SysContext";

        /// <summary>
        /// Redis配置
        /// </summary>
        public const string RedisCache = "RedisCache";

        /// <summary>
        /// 微信配置
        /// </summary>
        public const string Weixin = "Weixin";

        /// <summary>
        /// 微信小程序配置
        /// </summary>
        public const string MiniProgram = "MiniProgram";

        /// <summary>
        /// 安全类型
        /// </summary>
        public const string SafeContentType = "SafeContentType";

        /// <summary>
        /// 安全类型
        /// </summary>
        public const string FileContentType = "FileContentType";

        /// <summary>
        /// 插件
        /// </summary>
        public const string PluginsPrefix = "PluginsPrefix";

        /// <summary>
        /// 上传路径
        /// </summary>
        public const string UploadDirectory = $"{SysContext}:UploadDirectory";

        /// <summary>
        /// 上传路径映射
        /// </summary>
        public const string UploadDirectoryMapPath = $"{SysContext}:UploadDirectoryMapPath";
    }
}