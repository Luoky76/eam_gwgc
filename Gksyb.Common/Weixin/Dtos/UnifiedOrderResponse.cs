using System.Xml.Serialization;

namespace Gksyb.Common.Weixin
{
    [Serializable]
    [XmlRoot("xml")]
    public class UnifiedOrderResponse
    {
        /// <summary>
        /// 状态
        /// </summary>
        [XmlElement("return_code")]
        public string ReturnCode { get; set; }

        /// <summary>
        /// 结果明细
        /// </summary>
        [XmlElement("return_msg")]
        public string ReturnMsg { get; set; }

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
        ///JSAPI--JSAPI支付（或小程序支付）、NATIVE--Native支付、APP--app支付，MWEB--H5支付
        /// </summary>
        [XmlElement("trade_type")]
        public string TradeType { get; set; }

        /// <summary>
        ///签名
        /// </summary>
        [XmlElement("sign")]
        public string Sign { get; set; }

        /// <summary>
        ///业务结果
        /// </summary>
        [XmlElement("result_code")]
        public string ResultCode { get; set; }

        /// <summary>
        ///错误代码
        /// </summary>
        [XmlElement("err_code")]
        public string ErrCode { get; set; }

        /// <summary>
        ///错误代码描述
        /// </summary>
        [XmlElement("err_code_des")]
        public string ErrMsg { get; set; }

        /// <summary>
        ///状态
        /// </summary>
        [XmlElement("prepay_id")]
        public string PrepayId { get; set; }

        /// <summary>
        /// 是否错误
        /// </summary>
        public bool IsError
        {
            get
            {
                return string.IsNullOrWhiteSpace(PrepayId);
            }
        }

        public override string ToString()
        {
            return $"{ErrCode}:{ErrMsg}{(ReturnCode == "SUCCESS" ? "" : ReturnMsg)}";
        }
    }
}