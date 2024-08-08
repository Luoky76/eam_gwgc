using Newtonsoft.Json;

namespace Gksyb.Common.Weixin
{
    /// <summary>
    /// JSAPI下单
    /// </summary>
    public class JsApiTransactionsRequest
    {
        public JsApiTransactionsRequest(string appid, string mchid, WeixinTransactionsRequest request)
        {
            Appid = appid;
            Mchid = mchid;
            Description = request.Description;
            OutTradeNo = request.OutTradeNo;
            NotifyUrl = request.NotifyUrl;
            Amount = new TransactionsAmountV3()
            {
                Total = request.Amount
            };
            Payer = new TransactionsPayer()
            {
                Openid = request.Openid
            };
            if (request.ExpiresIn.HasValue)
            {
                TimeExpire = DateTimeOffset.Now.AddMinutes(request.ExpiresIn.Value).ToString("yyyy-MM-ddTHH:mm:sszzz");
            }
        }

        /// <summary>
        /// 公众账号ID
        /// </summary>
        [JsonProperty("appid")]
        public string Appid { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        [JsonProperty("mchid")]
        public string Mchid { get; set; }

        /// <summary>
        /// 商品描述
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// 商户系统内部订单号，只能是数字、大小写字母_-*且在同一个商户号下唯一 string[6,32]
        /// </summary>
        [JsonProperty("out_trade_no")]
        public string OutTradeNo { get; set; }

        /// <summary>
        ///异步接收微信支付结果通知的回调地址，通知url必须为外网可访问的url，不能携带参数。 公网域名必须为https，如果是走专线接入，使用专线NAT IP或者私有回调域名可使用http
        /// </summary>
        [JsonProperty("notify_url")]
        public string NotifyUrl { get; set; }

        /// <summary>
        ///订单金额信息
        /// </summary>
        [JsonProperty("amount")]
        public TransactionsAmountV3 Amount { get; set; }

        /// <summary>
        ///支付者信息
        /// </summary>
        [JsonProperty("payer")]
        public TransactionsPayer Payer { get; set; }

        /// <summary>
        ///订单失效时间，建议：最短失效时间间隔大于1分钟 示例值：2018-06-08T10:34:56+08:00
        /// </summary>
        [JsonProperty("time_expire")]
        public string TimeExpire { get; set; }

        /// <summary>
        /// 附加数据，在查询API和支付通知中原样返回，可作为自定义参数使用
        /// </summary>
        [JsonProperty("attach")]
        public string Attach { get; set; }

        /// <summary>
        ///订单优惠标记
        /// </summary>
        [JsonProperty("goods_tag")]
        public string GoodsTag { get; set; }

        /// <summary>
        ///电子发票入口开放标识
        /// </summary>
        [JsonProperty("support_fapiao")]
        public string SupportFapiao { get; set; }

        /// <summary>
        ///优惠功能 具体查看https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter3_1_1.shtml
        /// </summary>
        [JsonProperty("detail")]
        public object Detail { get; set; }

        /// <summary>
        ///支付场景描述 具体查看https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter3_1_1.shtml
        /// </summary>
        [JsonProperty("scene_info")]
        public object SceneInfo { get; set; }

        /// <summary>
        ///支付场景描述 具体查看https://pay.weixin.qq.com/wiki/doc/apiv3/apis/chapter3_1_1.shtml
        /// </summary>
        [JsonProperty("settle_info")]
        public object SettleInfo { get; set; }
    }

    /// <summary>
    /// 金额
    /// </summary>
    public class TransactionsAmountV3
    {
        /// <summary>
        /// 订单总金额，单位为分。
        /// </summary>
        [JsonProperty("total")]
        public int Total { get; set; }

        /// <summary>
        ///CNY：人民币，境内商户号仅支持人民币。
        /// </summary>
        [JsonProperty("currency")]
        public string Currency { get; set; } = "CNY";
    }

    /// <summary>
    /// 金额
    /// </summary>
    public class TransactionsAmount: TransactionsAmountV3
    {
        /// <summary>
        /// 订单总金额，单位为分。
        /// </summary>
        [JsonProperty("payer_total")]
        public int PayerTotal { get; set; }
    }

    /// <summary>
    /// 支付者信息
    /// </summary>
    public class TransactionsPayer
    {
        /// <summary>
        /// 用户的Openid
        /// </summary>
        [JsonProperty("openid")]
        public string Openid { get; set; }
    }
}