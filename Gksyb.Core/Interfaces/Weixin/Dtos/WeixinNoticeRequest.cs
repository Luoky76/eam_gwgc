using Gksyb.Common.Weixin;

namespace Gksyb.Core.Interfaces.Weixin
{
    public class WeixinNoticeRequest
    {
        /// <summary>
        /// 接收人
        /// </summary>
        public string Receiver { get; set; }

        /// <summary>
        /// 微信ID
        /// </summary>
        public string Openid { get; set; }

        /// <summary>
        /// 模板ID
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// 跳转链接
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// <see cref="WeiXinTemplateData"/>的json数据
        /// </summary>
        public string TData { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string Creater { get; set; }

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime? SendTime { get; set; }
    }
}