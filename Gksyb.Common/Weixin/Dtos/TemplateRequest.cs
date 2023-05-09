#pragma warning disable IDE1006 // 命名样式

namespace Gksyb.Common.Weixin
{
    public class TemplateRequest
    {
        public TemplateRequest()
        {
            topcolor = "#FF0000";
        }

        /// <summary>
        /// 接收者openid
        /// </summary>
        public string touser { get; set; }

        /// <summary>
        /// 模板ID
        /// </summary>
        public string template_id { get; set; }

        /// <summary>
        /// 模板消息顶部颜色（16进制），默认为#FF0000
        /// </summary>
        public string topcolor { get; set; }

        /// <summary>
        /// 模板跳转链接
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// 跳小程序所需数据，不需跳小程序可不用传该数据
        /// </summary>
        public MiniProgramTemplateRequest miniprogram { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public object data { get; set; }
    }

    /// <summary>
    /// 跳小程序所需数据
    /// </summary>
    public class MiniProgramTemplateRequest
    {
        /// <summary>
        /// 所需跳转到的小程序appid（该小程序appid必须与发模板消息的公众号是绑定关联关系，暂不支持小游戏）
        /// </summary>
        public string appid { get; set; }

        /// <summary>
        /// 所需跳转到小程序的具体页面路径，支持带参数,（示例index?foo=bar），要求该小程序已发布，暂不支持小游戏
        /// </summary>
        public string pagepath { get; set; }
    }
}