namespace Gksyb.Common.EventBus
{
    /// <summary>
    /// 事件发布者
    /// </summary>
    internal sealed class EventPublisher : IEventPublisher
    {
        private readonly IEventStorer _eventStorer;

        /// <summary>
        /// 构造函数
        /// </summary>
        public EventPublisher(IEventStorer eventStorer)
        {
            _eventStorer = eventStorer;
        }

        public async Task PublishAsync(ActionData data)
        {
            await _eventStorer.WriteAsync(data);
        }

        public async Task BroadcastAsync(ActionData data)
        {
            await _eventStorer.BroadcastAsync(data);
        }

        public async Task MessageQueueAsync(ActionData data)
        {
            await _eventStorer.MessageQueueAsync(data);
        }
    }
}