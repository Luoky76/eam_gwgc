using Newtonsoft.Json;
using System.Globalization;
using System.Xml;

namespace Gksyb.Common.Weixin
{
    /// <summary>
    /// 支付单结果
    /// </summary>
    public class TransactionsStatusResponse
    {
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
        /// 商户系统内部订单号，只能是数字、大小写字母_-*且在同一个商户号下唯一 string[6,32]
        /// </summary>
        [JsonProperty("out_trade_no")]
        public string OutTradeNo { get; set; }

        /// <summary>
        /// 微信支付系统生成的订单号。
        /// </summary>
        [JsonProperty("transaction_id")]
        public string TransactionId { get; set; }

        /// <summary>
        ///JSAPI--JSAPI支付（或小程序支付）、NATIVE--Native支付、APP--app支付，MWEB--H5支付
        /// </summary>
        [JsonProperty("trade_type")]
        public string TradeType { get; set; }

        /// <summary>
        /// 交易状态
        /// 交易状态，枚举值：
        ///SUCCESS：支付成功
        /// REFUND：转入退款
        /// NOTPAY：未支付
        /// CLOSED：已关闭
        /// REVOKED：已撤销（付款码支付）
        /// USERPAYING：用户支付中（付款码支付）
        /// PAYERROR：支付失败(其他原因，如银行返回失败)
        /// 示例值：SUCCESS
        /// </summary>
        [JsonProperty("trade_state")]
        public string TradeState { get; set; }

        /// <summary>
        /// 交易状态描述
        /// </summary>
        [JsonProperty("trade_state_desc")]
        public string TradeStateDesc { get; set; }

        /// <summary>
        /// 付款银行
        /// 银行类型，采用字符串类型的银行标识。
        /// 银行标识请参考 https://pay.weixin.qq.com/wiki/doc/apiv3/terms_definition/chapter1_1_3.shtml#part-6
        /// 示例值：CMC
        /// </summary>
        [JsonProperty("bank_type")]
        public string BankType { get; set; }

        /// <summary>
        /// 附加数据，在查询API和支付通知中原样返回，可作为自定义参数使用
        /// </summary>
        [JsonProperty("attach")]
        public string Attach { get; set; }

        /// <summary>
        /// 支付完成时间，遵循rfc3339标准格式，格式为YYYY-MM-DDTHH:mm:ss+TIMEZONE，YYYY-MM-DD表示年月日，T出现在字符串中，表示time元素的开头，HH:mm:ss表示时分秒，TIMEZONE表示时区（+08:00表示东八区时间，领先UTC 8小时，即北京时间）。例如：2015-05-20T13:29:35+08:00表示，北京时间2015年5月20日 13点29分35秒。
        /// </summary>
        [JsonProperty("success_time")]
        public DateTime? SuccessTime { get; set; }

        /// <summary>
        ///订单金额信息
        /// </summary>
        [JsonProperty("amount")]
        public TransactionsAmount Amount { get; set; }

        /// <summary>
        ///支付者信息
        /// </summary>
        [JsonProperty("payer")]
        public TransactionsPayer Payer { get; set; }

        /// <summary>
        /// 是否错误
        /// </summary>
        public bool IsError
        {
            get
            {
                return TradeState != "SUCCESS";
            }
        }

        /// <summary>
        /// XML返回
        /// </summary>
        /// <returns></returns>
        public static TransactionsStatusResponse FromXml(string content)
        {
            var xmlDoc = new XmlDocument()
            {
                XmlResolver = null
            };
            xmlDoc.LoadXml(content);
            var response = new TransactionsStatusResponse();
            var xmlNode = xmlDoc["xml"];
            response.Appid = xmlNode["appid"]?.InnerText;
            response.Mchid = xmlNode["mch_id"]?.InnerText;
            response.OutTradeNo = xmlNode["out_trade_no"]?.InnerText;
            response.TransactionId = xmlNode["transaction_id"]?.InnerText;
            response.TradeType = xmlNode["trade_type"]?.InnerText;
            var returnCode = xmlNode["return_code"]?.InnerText;
            var resultCode = xmlNode["result_code"]?.InnerText;
            var errCode = xmlNode["err_code"]?.InnerText;
            response.TradeState = (returnCode == "SUCCESS" && resultCode == "SUCCESS") ? "SUCCESS" : $"{returnCode}-{resultCode}-{errCode}";
            response.TradeStateDesc = xmlNode["err_code_des"]?.InnerText;
            response.BankType = xmlNode["bank_type"]?.InnerText;
            response.Attach = xmlNode["attach"]?.InnerText;
            var timeEnd = xmlNode["time_end"]?.InnerText;
            if (!string.IsNullOrWhiteSpace(timeEnd)) response.SuccessTime = DateTime.ParseExact(timeEnd, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
            response.Amount = new TransactionsAmount()
            {
                Total = (xmlNode["total_fee"]?.InnerText ?? "0").CastTo(0),
                PayerTotal = (xmlNode["cash_fee"]?.InnerText ?? "0").CastTo(0),
                Currency = xmlNode["fee_type"]?.InnerText
            };
            response.Payer = new TransactionsPayer()
            {
                Openid = xmlNode["openid"]?.InnerText
            };
            return response;
        }

        /// <summary>
        /// XML返回
        /// </summary>
        /// <returns></returns>
        public static TransactionsStatusResponse FromPayV3(string content)
        {
            return FromXml(content);
        }

        /// <summary>
        /// 返回错误实例
        /// </summary>
        /// <returns></returns>
        public static TransactionsStatusResponse Error(string message)
        {
            return new TransactionsStatusResponse()
            {
                TradeState = "ERROR",
                TradeStateDesc = message
            };
        }
    }
}