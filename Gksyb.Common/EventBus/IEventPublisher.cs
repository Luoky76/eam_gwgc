namespace Gksyb.Common.EventBus
{
    /// <summary>
    /// 事件发布
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// 发布事件
        /// </summary>
        Task PublishAsync(ActionData data);

        /// <summary>
        /// 广播事件
        /// </summary>
        Task BroadcastAsync(ActionData data);

        /// <summary>
        /// 消息队列
        /// </summary>
        Task MessageQueueAsync(ActionData data);
    }
}