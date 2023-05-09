namespace Gksyb.Common.EventBus
{
    /// <summary>
    /// 事件处理程序特性
    /// </summary>
    /// <remarks>
    /// <para>作用于 <see cref="IEventSubscriber"/> 实现类实例方法</para>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public sealed class EventSubscribeAttribute : Attribute
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="eventId">事件 Id</param>
        public EventSubscribeAttribute(string eventId)
        {
            EventId = eventId;
        }

        /// <summary>
        /// 事件 Id
        /// </summary>
        public string EventId { get; set; }
    }
}