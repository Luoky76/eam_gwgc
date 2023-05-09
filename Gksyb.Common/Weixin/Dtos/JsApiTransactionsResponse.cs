using Newtonsoft.Json;

namespace Gksyb.Common.Weixin
{
    /// <summary>
    /// JSAPI下单
    /// </summary>
    public class JsApiTransactionsResponse : WeixinResponse
    {
        /// <summary>
        ///预支付交易会话标识。用于后续接口调用中使用，该值有效期为2小时
        /// </summary>
        [JsonProperty("prepay_id")]
        public string PrepayId { get; set; }
    }
}