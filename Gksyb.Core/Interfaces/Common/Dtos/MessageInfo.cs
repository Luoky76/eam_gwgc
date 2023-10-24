namespace Gksyb.Core.Interfaces.Common
{
    /// <summary>
    /// 消息基类
    /// </summary>
    public class MessageInfoBase
    {
        /// <summary>
        /// 消息主键
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 模板
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// 内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public object Data { get; set; }

        /// <summary>
        /// 弹窗模式
        /// </summary>
        public string DialogMode { get; set; }

        /// <summary>
        /// 弹窗类别
        /// </summary>
        public string DialogType { get; set; }

        /// <summary>
        /// 链接
        /// </summary>
        public string Href { get; set; }

        /// <summary>
        /// 链接目标
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// 移动端链接
        /// </summary>
        public string MobileHref { get; set; }

        /// <summary>
        /// 关联主键
        /// </summary>
        public string Key { get; set; }

        private Dictionary<string, object> _dic;

        /// <summary>
        /// 转字典数据
        /// </summary>
        public Dictionary<string, object> GetDicData(bool cache = true)
        {
            if (!cache) _dic = null;
            if (_dic != null) return _dic;
            Dictionary<string, object> dic = null;
            try
            {
                var json = Data is string ? Data as string : Data?.ToJson();
                dic = (json ?? "").ToObject<Dictionary<string, object>>();
            }
            catch
            {
            }
            dic ??= new Dictionary<string, object>();
            if (!dic.ContainsKey("Key")) dic.Add("Key", Key);
            _dic = dic.ToIgnoreCaseDictionary();
            return _dic;
        }
    }

    /// <summary>
    /// 消息
    /// </summary>
    public class MessageInfo : MessageInfoBase
    {
        /// <summary>
        /// 消息编码
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 消息类型（Message:站内信,Weixin:微信,Sms:短信）
        /// </summary>
        public string MsgType { get; set; }

        /// <summary>
        /// 消息组
        /// </summary>
        public string MsgGroup { get; set; }

        /// <summary>
        /// 处理方法
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// 自动已读 {1：一次性消息不写入数据库}
        /// </summary>
        public string AutoReaded { get; set; } = "1";

        /// <summary>
        /// 发送时间
        /// </summary>
        public DateTime? SendTime { get; set; }

        /// <summary>
        /// 组织
        /// </summary>
        public string CorpId { get; set; }

        /// <summary>
        /// 接收人
        /// </summary>
        public List<string> Receives { get; set; }

        /// <summary>
        /// 接收组
        /// </summary>
        public List<string> Groups { get; set; }

        /// <summary>
        /// 应用名
        /// </summary>
        public string Appname { get; set; }
    }

    /// <summary>
    /// 消息
    /// </summary>
    public class MessageInfo<T> : MessageInfo
    {
        public new T Data
        {
            get { return (T)base.Data; }
            set { base.Data = value; }
        }
    }
}