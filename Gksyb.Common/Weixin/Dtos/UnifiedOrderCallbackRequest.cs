using System.Xml;
using System.Xml.Serialization;

namespace Gksyb.Common.Weixin
{
    [Serializable]
    [XmlRoot("xml")]
    public class UnifiedOrderCallbackRequest
    {
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
        ///随机字符串
        /// </summary>
        [XmlElement("nonce_str")]
        public string NonceStr { get; set; }

        /// <summary>
        /// 付款银行
        /// 银行类型，采用字符串类型的银行标识。
        /// 银行标识请参考 https://pay.weixin.qq.com/wiki/doc/apiv3/terms_definition/chapter1_1_3.shtml#part-6
        /// 示例值：CMC
        /// </summary>
        [XmlElement("bank_type")]
        public string BankType { get; set; }

        /// <summary>
        /// 现金支付金额订单现金支付金额
        /// </summary>
        [XmlElement("cash_fee")]
        public string CashFee { get; set; }

        /// <summary>
        /// 货币类型，符合ISO4217标准的三位字母代码，默认人民币：CNY
        /// </summary>
        [XmlElement("fee_type")]
        public string FeeType { get; set; }

        /// <summary>
        /// 用户是否关注公众账号，Y-关注，N-未关注
        /// </summary>
        [XmlElement("is_subscribe")]
        public string IsSubscribe { get; set; }

        /// <summary>
        ///支付者信息
        /// </summary>
        [XmlElement("openid")]
        public string Openid { get; set; }

        /// <summary>
        /// 商户系统内部订单号，只能是数字、大小写字母_-*且在同一个商户号下唯一 string[6,32]
        /// </summary>
        [XmlElement("out_trade_no")]
        public string OutTradeNo { get; set; }

        /// <summary>
        /// 通讯结果
        /// </summary>
        [XmlElement("return_code")]
        public string ReturnCode { get; set; }

        /// <summary>
        /// 通讯结果明细
        /// </summary>
        [XmlElement("return_msg")]
        public string ReturnMsg { get; set; }

        /// <summary>
        ///交易结果
        /// </summary>
        [XmlElement("result_code")]
        public string ResultCode { get; set; }

        /// <summary>
        ///状态
        /// </summary>
        [XmlElement("err_code")]
        public string ErrCode { get; set; }

        /// <summary>
        ///状态
        /// </summary>
        [XmlElement("err_code_des")]
        public string ErrCodeDes { get; set; }

        /// <summary>
        /// 签名 https://pay.weixin.qq.com/wiki/doc/api/jsapi.php?chapter=4_3
        /// </summary>
        [XmlElement("sign")]
        public string Sign { get; set; }

        /// <summary>
        /// 支付完成时间，格式为yyyyMMddHHmmss，如2009年12月25日9点10分10秒表示为20091225091010
        /// </summary>
        [XmlElement("time_end")]
        public string TimeEnd { get; set; }

        /// <summary>
        /// 订单总金额，单位为分
        /// </summary>
        [XmlElement("total_fee")]
        public ushort TotalFee { get; set; }

        /// <summary>
        ///JSAPI--JSAPI支付（或小程序支付）、NATIVE--Native支付、APP--app支付，MWEB--H5支付
        /// </summary>
        [XmlElement("trade_type")]
        public string TradeType { get; set; }

        /// <summary>
        /// 微信支付系统生成的订单号。
        /// </summary>
        [XmlElement("transaction_id")]
        public string TransactionId { get; set; }

        /// <summary>
        /// 是否错误
        /// </summary>
        public bool IsError
        {
            get
            {
                return ReturnCode == "SUCCESS" && ResultCode == "SUCCESS";
            }
        }

        /// <summary>
        /// 错误描述
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{ErrCode}:{ErrCodeDes}";
        }
    }
}