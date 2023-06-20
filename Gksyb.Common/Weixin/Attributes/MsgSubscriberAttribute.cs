namespace Gksyb.Common.Weixin
{
    /// <summary>
    /// 微信消息处理程序特性
    /// </summary>
    /// <remarks>
    /// <para>作用于 <see cref="IMsgSubscriber"/> 实现类实例方法</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class MsgSubscriberAttribute : Attribute
    {
        /// <summary>
        /// 消息类型
        /// </summary>
        public string MsgType { get; set; }

        /// <summary>
        /// 事件类型
        /// </summary>
        public string Event { get; set; }
    }
}