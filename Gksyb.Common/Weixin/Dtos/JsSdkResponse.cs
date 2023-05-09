namespace Gksyb.Common.Weixin
{
    /// <summary>
    /// JSSDK
    /// </summary>
    public class JsSdkResponse
    {
        /// <summary>
        /// 微信AppId
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public string Timestamp { get; set; }

        /// <summary>
        /// 随机码
        /// </summary>
        public string NonceStr { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string Signature { get; set; }

        /// <summary>
        /// 获取JSSDK
        /// </summary>
        /// <returns></returns>
        public static JsSdkResponse GetInstance(string ticket, string url)
        {
            var response = new JsSdkResponse
            {
                AppId = WeixinSetting.AppId,
                Timestamp = (DateTimeOffset.Now - DateTimeOffset.UnixEpoch).TotalSeconds.CastTo<long>().ToString(),
                NonceStr = Guid.NewGuid().ToString("N").ToLower()
            };
            var content = new[] {
                $"jsapi_ticket={ticket}",
                $"timestamp={response.Timestamp}",
                $"noncestr={response.NonceStr}",
                $"url={url}"}.OrderBy(c => c).ToStr("&");
            response.Signature = CryptographyHelper.GetSha1(content).ToLower();
            return response;
        }
    }
}