using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Common.Weixin
{
    public class WeixinRequest
    {
        /// <summary>
        /// 微信加密签名
        /// </summary>
        [FromQuery]
        public string Signature { get; set; }

        /// <summary>
        /// 随机字符串
        /// </summary>
        [FromQuery]
        public string Echostr { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        [FromQuery]
        public string Timestamp { get; set; }

        /// <summary>
        /// 随机数
        /// </summary>
        [FromQuery]
        public string Nonce { get; set; }

        /// <summary>
        /// 微信ID
        /// </summary>
        [FromQuery]
        public string Openid { get; set; }

        /// <summary>
        /// XML内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 验证
        /// </summary>
        /// <returns></returns>
        public bool Check()
        {
            var signature = GetSignature();
            return Signature == signature;
        }

        /// <summary>
        /// 获取签名
        /// </summary>
        /// <returns></returns>
        private string GetSignature()
        {
            var content = new[] { WeixinSetting.Token, Timestamp, Nonce }.OrderBy(c => c).ToStr("");
            return CryptographyHelper.GetSha1(content).ToLower();
        }
    }
}