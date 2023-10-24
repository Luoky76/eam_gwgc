using Newtonsoft.Json;

namespace Gksyb.Common.Weixin
{
    /// <summary>
    /// 用户手机号信息
    /// </summary>
    [Serializable]
    public class UserPhoneResponse : WeixinResponse
    {
        /// <summary>
        /// 用户手机号信息
        /// </summary>
        [JsonProperty("phone_info")]
        public PhoneInfo PhoneInfo { get; set; }
    }

    public class PhoneInfo
    {
        /// <summary>
        /// 用户绑定的手机号（国外手机号会有区号）
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// 没有区号的手机号
        /// </summary>
        public string PurePhoneNumber { get; set; }

        /// <summary>
        /// 区号
        /// </summary>
        public int CountryCode { get; set; }

        /// <summary>
        /// 数据水印
        /// </summary>
        public Watermark Watermark { get; set; }
    }

    /// <summary>
    /// 数据水印
    /// </summary>
    public class Watermark
    {
        /// <summary>
        /// 用户获取手机号操作的时间戳
        /// </summary>
        public int Timestamp { get; set; }

        /// <summary>
        /// 小程序appid
        /// </summary>
        public string Appid { get; set; }
    }
}