using System.ComponentModel;

namespace Gksyb.Common.Weixin
{
    /// <summary>
    /// 消息响应类型
    /// </summary>
    public enum ResponseMsgType
    {
        [Description("其他")]
        Other = -2,

        [Description("未知")]
        Unknown = -1,//未知类型

        [Description("文本")]
        Text = 0,

        [Description("单图文")]
        News = 1,

        [Description("音乐")]
        Music = 2,

        [Description("图片")]
        Image = 3,

        [Description("语音")]
        Voice = 4,

        [Description("视频")]
        Video = 5,

        [Description("多客服")]
        Transfer_Customer_Service = 6,

        //transfer_customer_service
        [Description("素材多图文")]
        MpNews = 7,//素材多图文

        //以下为延伸类型，微信官方并未提供具体的回复类型
        [Description("多图文")]
        MultipleNews = 106,

        [Description("位置")]
        LocationMessage = 107,//

        [Description("无回复")]
        NoResponse = 110,

        [Description("success")]
        SuccessResponse = 200,

        [Description("使用API回复")]
        UseApi = 998,//使用接口访问
    }

    /// <summary>
    /// 响应回复消息基类
    /// </summary>
    public class MessageBaseResponse : MessageBase
    {
        public virtual ResponseMsgType MsgType { get; set; }

        public MessageBaseResponse()
        {
        }
    }
}