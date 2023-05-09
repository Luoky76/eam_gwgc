using Newtonsoft.Json;
using System.Web;

namespace Gksyb.Common.Weixin
{
    /// <summary>
    /// 下单返回
    /// </summary>
    public class WeixinTransactionsResponse
    {
        /// <summary>
        /// 微信AppId
        /// </summary>
        [JsonProperty("appId")]
        public string AppId { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        [JsonProperty("timeStamp")]
        public string Timestamp { get; set; }

        /// <summary>
        /// 随机码
        /// </summary>
        [JsonProperty("nonceStr")]
        public string NonceStr { get; set; }

        /// <summary>
        /// 统一支付接口返回的prepay_id参数值，提交格式如：prepay_id=\*\*\*）
        /// </summary>
        [JsonProperty("package")]
        public string Package { get; set; }

        /// <summary>
        /// 微信支付V3的传入RSA,微信支付V2的传入格式与V2统一下单的签名格式保持一致
        /// </summary>
        [JsonProperty("signType")]
        public string SignType { get; set; }

        /// <summary>
        /// 支付签名
        /// </summary>
        [JsonProperty("paySign")]
        public string PaySign { get; set; }

        /// <summary>
        /// 获取JSSDK
        /// </summary>
        /// <returns></returns>
        public static WeixinTransactionsResponse GetInstance(string prepayId, string nonceStr, string signType)
        {
            var response = new WeixinTransactionsResponse
            {
                AppId = WeixinSetting.AppId,
                Timestamp = (DateTimeOffset.Now - DateTimeOffset.UnixEpoch).TotalSeconds.CastTo<long>().ToString(),
                NonceStr = nonceStr,
                Package = HttpUtility.UrlEncode($"prepay_id={prepayId}"),
                SignType = signType
            };
            var content = new[] {
                $"appId={response.AppId}",
                $"timeStamp={response.Timestamp}",
                $"nonceStr={response.NonceStr}",
                $"package={response.Package}",
                $"signType={response.SignType}"}.OrderBy(c => c).ToStr("&");
            content = $"{content}&key={WeixinSetting.PayKey}";
            response.PaySign = CryptographyHelper.GetMd5(content).ToUpper();
            return response;
        }
    }
}