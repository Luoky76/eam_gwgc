using Newtonsoft.Json;

namespace Gksyb.Common.Weixin
{
    /// <summary>
    /// 下单
    /// </summary>
    public class WeixinTransactionsRequest
    {
        public WeixinTransactionsRequest()
        {
        }

        /// <summary>
        /// 微信订单
        /// </summary>
        /// <param name="outTradeNo">商户系统内部订单号，只能是数字、大小写字母_-*且在同一个商户号下唯一 string[6,32]</param>
        /// <param name="description">商品描述</param>
        /// <param name="notifyUrl">异步接收微信支付结果通知的回调地址</param>
        /// <param name="amount">订单金额信息 单位分</param>
        /// <param name="openid">支付者信息</param>
        /// <param name="expiresIn">订单超时时间，单位（分钟）</param>
        public WeixinTransactionsRequest(string outTradeNo, string description, string notifyUrl, int amount, string openid, int? expiresIn = null)
        {
            OutTradeNo = outTradeNo;
            Description = description;
            NotifyUrl = notifyUrl;
            Amount = amount;
            Openid = openid;
            ExpiresIn = expiresIn;
        }

        /// <summary>
        /// 商品描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 商户系统内部订单号，只能是数字、大小写字母_-*且在同一个商户号下唯一 string[6,32]
        /// </summary>
        public string OutTradeNo { get; set; }

        /// <summary>
        ///异步接收微信支付结果通知的回调地址，通知url必须为外网可访问的url，不能携带参数。 公网域名必须为https，如果是走专线接入，使用专线NAT IP或者私有回调域名可使用http
        /// </summary>
        public string NotifyUrl { get; set; }

        /// <summary>
        ///订单金额信息 单位分
        /// </summary>
        [JsonProperty("amount")]
        public int Amount { get; set; }

        /// <summary>
        ///支付者信息
        /// </summary>
        public string Openid { get; set; }

        /// <summary>
        /// 订单超时时间，单位（分钟）
        /// </summary>
        public int? ExpiresIn { get; set; }
    }
}