using Newtonsoft.Json;

namespace Gksyb.Common.Weixin
{
    /// <summary>
    /// 小程序code2Session登录凭据
    /// </summary>
    [Serializable]
    public class SessionResponse : WeixinResponse
    {
        /// <summary>
        /// 会话密钥
        /// </summary>
        [JsonProperty("session_key")]
        public string SessionKey { get; set; }

        /// <summary>
        /// 授权用户唯一标识
        /// </summary>
        public string Openid { get; set; }
    }
}