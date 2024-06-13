namespace Gksyb.Common.EventBus
{
    /// <summary>
    /// 事件存储器
    /// </summary>
    public interface IEventStorer
    {
        /// <summary>
        /// 事件写入
        /// </summary>
        ValueTask WriteAsync(ActionData data);

        /// <summary>
        /// 事件广播
        /// </summary>
        /// <returns></returns>
        ValueTask BroadcastAsync(ActionData data);

        /// <summary>
        /// 消息队列写入，支持消息保存
        /// </summary>
        /// <returns></returns>
        ValueTask MessageQueueAsync(ActionData data);

        /// <summary>
        /// 事件读取
        /// </summary>
        ValueTask<string> ReadAsync(CancellationToken cancellationToken);
    }
}