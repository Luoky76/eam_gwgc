namespace Gksyb.Common.Weixin
{
    /// <summary>
    /// 消息返回
    /// </summary>
    [Serializable]
    public class TemplateMessageResponse : WeixinResponse
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public long Msgid { get; set; }
    }
}