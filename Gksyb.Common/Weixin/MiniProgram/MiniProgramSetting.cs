using Microsoft.Extensions.Configuration;

namespace Gksyb.Common.Weixin
{
    public class MiniProgramSetting
    {
        /// <summary>
        /// 应用ID
        /// </summary>
        public static string AppId { get; set; }

        /// <summary>
        /// 应用密钥
        /// </summary>
        public static string AppSecret { get; set; }

        /// <summary>
        /// 微信配置初始化
        /// </summary>
        public static void InitFromConifg(IConfigurationSection config)
        {
            AppId = config["AppId"];
            AppSecret = config["AppSecret"];
        }
    }
}