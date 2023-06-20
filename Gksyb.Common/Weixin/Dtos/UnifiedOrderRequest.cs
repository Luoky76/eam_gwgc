using Chloe.Reflection;
using Chloe.Reflection.Emit;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;

namespace Gksyb.Common.Weixin
{
    [Serializable]
    [XmlRoot("xml")]
    public class UnifiedOrderRequest
    {
        public UnifiedOrderRequest()
        {
        }

        public UnifiedOrderRequest(string appid, string mchid, WeixinTransactionsRequest request)
        {
            Appid = appid;
            Mchid = mchid;
            Body = request.Description;
            OutTradeNo = request.OutTradeNo;
            NotifyUrl = request.NotifyUrl;
            TotalFee = request.Amount;
            Openid = request.Openid;
            if (request.ExpiresIn.HasValue)
            {
                TimeExpire = DateTimeOffset.Now.AddMinutes(request.ExpiresIn.Value).ToString("yyyyMMddHHmmss");
            }
        }

        /// <summary>
        /// 公众账号ID
        /// </summary>
        [XmlElement("appid")]
        public string Appid { get; set; }

        /// <summary>
        /// 商户号
        /// </summary>
        [XmlElement("mch_id")]
        public string Mchid { get; set; }

        /// <summary>
        ///商品描述
        /// </summary>
        [XmlElement("body")]
        public string Body { get; set; }

        /// <summary>
        /// 商户系统内部订单号，只能是数字、大小写字母_-*且在同一个商户号下唯一 string[6,32]
        /// </summary>
        [XmlElement("out_trade_no")]
        public string OutTradeNo { get; set; }

        /// <summary>
        ///订单金额信息
        /// </summary>
        [XmlElement("total_fee")]
        public int TotalFee { get; set; }

        /// <summary>
        /// 支持IPV4和IPV6两种格式的IP地址。用户的客户端IP
        /// </summary>
        [XmlElement("spbill_create_ip")]
        public string SpbillCreateIP { get; set; }

        /// <summary>
        ///异步接收微信支付结果通知的回调地址，通知url必须为外网可访问的url，不能携带参数。 公网域名必须为https，如果是走专线接入，使用专线NAT IP或者私有回调域名可使用http
        /// </summary>
        [XmlElement("notify_url")]
        public string NotifyUrl { get; set; }

        /// <summary>
        ///支付者信息
        /// </summary>
        [XmlElement("openid")]
        public string Openid { get; set; }

        /// <summary>
        ///JSAPI--JSAPI支付（或小程序支付）、NATIVE--Native支付、APP--app支付，MWEB--H5支付
        /// </summary>
        [XmlElement("trade_type")]
        public string TradeType { get; set; } = "JSAPI";

        /// <summary>
        ///随机字符串
        /// </summary>
        [XmlElement("nonce_str")]
        public string NonceStr { get; set; }

        /// <summary>
        ///签名
        /// </summary>
        [XmlElement("sign")]
        public string Sign { get; set; }

        /// <summary>
        ///签名类型，默认为MD5，支持HMAC-SHA256和MD5。
        /// </summary>
        [XmlElement("sign_type")]
        public string SignType { get; set; } = "MD5";

        /// <summary>
        ///设备号
        /// </summary>
        [XmlElement("device_info")]
        public string DeviceInfo { get; set; } = "WEB";

        /// <summary>
        ///商品详细描述，对于使用单品优惠的商户，该字段必须按照规范上传，详见“单品优惠参数说明”
        /// </summary>
        [XmlElement("detail")]
        public string Detail { get; set; }

        /// <summary>
        ///附加数据，在查询API和支付通知中原样返回，可作为自定义参数使用。
        /// </summary>
        [XmlElement("attach")]
        public string Attach { get; set; }

        /// <summary>
        ///符合ISO 4217标准的三位字母代码，默认人民币：CNY
        /// </summary>
        [XmlElement("fee_type")]
        public string FeeType { get; set; } = "CNY";

        /// <summary>
        ///订单生成时间，格式为yyyyMMddHHmmss，如2009年12月25日9点10分10秒表示为20091225091010
        /// </summary>
        [XmlElement("time_start")]
        public string TimeStart { get; set; }

        /// <summary>
        ///订单失效时间，格式为yyyyMMddHHmmss，如2009年12月27日9点10分10秒表示为20091227091010。订单失效时间是针对订单号而言的，由于在请求支付的时候有一个必传参数prepay_id只有两小时的有效期，所以在重入时间超过2小时的时候需要重新请求下单接口获取新的prepay_id
        /// </summary>
        [XmlElement("time_expire")]
        public string TimeExpire { get; set; }

        /// <summary>
        ///订单优惠标记，使用代金券或立减优惠功能时需要的参数
        /// </summary>
        [XmlElement("goods_tag")]
        public string GoodsTag { get; set; }

        /// <summary>
        ///trade_type=NATIVE时，此参数必传。此参数为二维码中包含的商品ID，商户自行定义
        /// </summary>
        [XmlElement("product_id")]
        public string ProductId { get; set; }

        /// <summary>
        ///上传此参数no_credit--可限制用户不能使用信用卡支付
        /// </summary>
        [XmlElement("no_credit")]
        public string NoCredit { get; set; }

        /// <summary>
        ///传入Y时，支付成功消息和支付详情页将出现开票入口。需要在微信支付商户平台或微信公众平台开通电子发票功能，传此字段才可生效
        /// </summary>
        [XmlElement("receipt")]
        public string Receipt { get; set; }

        /// <summary>
        ///Y-是，需要分账 N-否，不分账
        /// </summary>
        [XmlElement("profit_sharing")]
        public string ProfitSharing { get; set; }

        /// <summary>
        ///该字段常用于线下活动时的场景信息上报，支持上报实际门店信息，商户也可以按需求自己上报相关信息。该字段为JSON对象数据
        /// </summary>
        [XmlElement("scene_info")]
        public string SceneInfo { get; set; }

        /// <summary>
        /// 计算Sign
        /// </summary>
        public void ComputeSign(string key)
        {
            var methods = MemberGets;
            var list = new List<string>();
            foreach (var method in methods)
            {
                var value = method.Value(this);
                if (value == null) continue;
                list.Add($"{method.Key}={value}");
            }
            list.Sort();
            list.Add($"key={key}");
            Sign = CryptographyHelper.GetMd5(list.ToStr("&")).ToUpper();
        }

        /// <summary>
        /// 成员get方法
        /// </summary>
        private static Dictionary<string, MemberGetter> memberGets = null;

        /// <summary>
        /// 成员get方法
        /// </summary>
        private static Dictionary<string, MemberGetter> MemberGets
        {
            get
            {
                if (memberGets != null) return memberGets;
                memberGets = new Dictionary<string, MemberGetter>();
                var type = typeof(UnifiedOrderRequest);
                var memberInfos = new List<MemberInfo>(type.GetProperties());
                memberInfos.AddRange(type.GetFields());
                foreach (var memberInfo in memberInfos)
                {
                    var element = memberInfo.GetAttribute<XmlElementAttribute>();
                    if (element == null || string.IsNullOrWhiteSpace(element.ElementName)) continue;
                    var getter = DelegateGenerator.CreateGetter(memberInfo);
                    memberGets.Add(element.ElementName, getter);
                }
                return memberGets;
            }
        }
    }
}