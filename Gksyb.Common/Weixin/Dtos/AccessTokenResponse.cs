using Newtonsoft.Json;

namespace Gksyb.Common.Weixin
{
    /// <summary>
    /// access_token请求后的JSON返回格式
    /// </summary>
    [Serializable]
    public class AccessTokenResponse : WeixinResponse
    {
        /// <summary>
        /// 接口调用凭证
        /// </summary>
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        /// <summary>
        /// 接口调用凭证超时时间，单位（秒）
        /// </summary>
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpiresTime { get; set; }

        public void SeExpiresTime()
        {
            var expireInSeconds = ExpiresIn;
            if (expireInSeconds > 180)
            {
                expireInSeconds -= 180;//提前3分钟过期
            }
            ExpiresTime = DateTime.Now.AddSeconds(expireInSeconds);
        }

        public bool IsExpires => IsError || ExpiresTime <= DateTime.Now;
    }
}