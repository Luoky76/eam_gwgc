using Microsoft.Extensions.Configuration;

namespace Gksyb.Common.Weixin
{
    public class WeixinSetting
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
        /// 票据
        /// </summary>
        public static string Token { get; set; }

        /// <summary>
        /// AES密钥
        /// </summary>
        public static byte[] EncodingAESKey { get; set; }

        /// <summary>
        /// 商户ID用于微信支付
        /// </summary>
        public static string Mchid { get; set; }

        /// <summary>
        /// 支付密钥 用于统一下单接口
        /// </summary>
        public static string PayKey { get; set; }

        /// <summary>
        /// API v3密钥
        /// </summary>
        public static string PayApiKey { get; set; }

        /// <summary>
        /// 微信支付证书私钥
        /// </summary>
        public static string PayPrivateKey { get; set; }

        /// <summary>
        /// 微信支付证书序列号
        /// </summary>
        public static string PaySerialNumber { get; set; }

        /// <summary>
        /// 微信配置初始化
        /// </summary>
        public static void InitFromConifg(IConfigurationSection config)
        {
            AppId = config["AppId"];
            AppSecret = config["AppSecret"];
            Token = config["Token"];
            var aesKey = config["EncodingAESKey"];
            EncodingAESKey = string.IsNullOrWhiteSpace(aesKey) ? null : Convert.FromBase64String(aesKey + "=");
            Mchid = config["Mchid"];
            PayKey = config["PayKey"];
            PayApiKey = config["PayApiKey"];
            PayPrivateKey = config["PayPrivateKey"];
            PaySerialNumber = config["PaySerialNumber"];
        }
    }
}