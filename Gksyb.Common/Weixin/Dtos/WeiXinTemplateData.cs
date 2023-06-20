#pragma warning disable IDE1006 // 命名样式


namespace Gksyb.Common.Weixin
{
    /// <summary>
    /// 微信通知模板
    /// </summary>
    [Serializable]
    public class WeiXinTemplateData
    {
        public WeiXinTemplateDataItem first { get; set; }
        public WeiXinTemplateDataItem keyword1 { get; set; }
        public WeiXinTemplateDataItem keyword2 { get; set; }
        public WeiXinTemplateDataItem keyword3 { get; set; }
        public WeiXinTemplateDataItem keyword4 { get; set; }
        public WeiXinTemplateDataItem keyword5 { get; set; }
        public WeiXinTemplateDataItem keyword6 { get; set; }
        public WeiXinTemplateDataItem keyword7 { get; set; }
        public WeiXinTemplateDataItem keyword8 { get; set; }
        public WeiXinTemplateDataItem keyword9 { get; set; }
        public WeiXinTemplateDataItem remark { get; set; }
    }

    public class WeiXinTemplateDataItem
    {
        public WeiXinTemplateDataItem(string v, string c = "#173177")
        {
            value = v;
            color = c;
        }

        public string color { get; set; }
        public string value { get; set; }
    }
}