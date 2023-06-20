using Newtonsoft.Json;

namespace Gksyb.Common.Weixin
{
    /// <summary>
    /// 微信回调请求
    /// </summary>
    public class WeixinCallbackRequest
    {
        /// <summary>
        /// 通知的唯一ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 通知创建的时间，遵循rfc3339标准格式，格式为YYYY-MM-DDTHH:mm:ss+TIMEZONE，YYYY-MM-DD表示年月日，T出现在字符串中，表示time元素的开头，HH:mm:ss.表示时分秒，TIMEZONE表示时区（+08:00表示东八区时间，领先UTC 8小时，即北京时间）。例如：2015-05-20T13:29:35+08:00表示北京时间2015年05月20日13点29分35秒。
        /// </summary>
        [JsonProperty("create_time")]
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 通知的资源数据类型，支付成功通知为encrypt-resource
        /// </summary>
        [JsonProperty("resource_type")]
        public string ResourceType { get; set; }

        /// <summary>
        /// 通知的类型，支付成功通知的类型为TRANSACTION.SUCCESS
        /// </summary>
        [JsonProperty("event_type")]
        public string EventType { get; set; }

        /// <summary>
        /// 回调摘要
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// 通知资源数据
        /// </summary>
        public WeixinCallbackResource Resource { get; set; }

        /// <summary>
        /// 转成支付单统一结果
        /// </summary>
        /// <returns></returns>
        public TransactionsStatusResponse ToTransactionsStatusResponse()
        {
            var json = CryptographyHelper.AesGcmDecrypt(Resource.AssociatedData, Resource.Nonce, Resource.Ciphertext, WeixinSetting.PayApiKey);
            return json.ToObject<TransactionsStatusResponse>();
        }
    }

    public class WeixinCallbackResource
    {
        /// <summary>
        /// 原始回调类型，为transaction
        /// </summary>
        [JsonProperty("original_type")]
        public string OriginalType { get; set; }

        /// <summary>
        /// 对开启结果数据进行加密的加密算法，目前只支持AEAD_AES_256_GCM
        /// </summary>
        public string Algorithm { get; set; }

        /// <summary>
        /// Base64编码后的开启/停用结果数据密文
        /// </summary>
        public string Ciphertext { get; set; }

        /// <summary>
        /// 附加数据
        /// </summary>
        [JsonProperty("associated_data")]
        public string AssociatedData { get; set; }

        /// <summary>
        ///加密使用的随机串
        /// </summary>
        public string Nonce { get; set; }
    }
}