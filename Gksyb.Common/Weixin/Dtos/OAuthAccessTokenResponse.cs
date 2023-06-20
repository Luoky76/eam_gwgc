using Newtonsoft.Json;

namespace Gksyb.Common.Weixin
{
    /// <summary>
    /// 获取OAuth AccessToken的结果
    /// </summary>
    [Serializable]
    public class OAuthAccessTokenResponse : AccessTokenResponse
    {
        /// <summary>
        /// 用户刷新access_token
        /// </summary>
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        /// <summary>
        /// 授权用户唯一标识
        /// </summary>
        public string Openid { get; set; }

        /// <summary>
        /// 用户授权的作用域，使用逗号（,）分隔
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// 只有在用户将公众号绑定到微信开放平台帐号后，才会出现该字段。详见：获取用户个人信息（UnionID机制）
        /// </summary>
        public string Unionid { get; set; }
    }
}