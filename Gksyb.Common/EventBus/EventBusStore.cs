namespace Gksyb.Common.EventBus
{
    public static class EventBusStore
    {
        /// <summary>
        /// 事件处理程序集合
        /// </summary>
        public static HashSet<EventHandler> EventHandlers { get; set; } = new();
    }
}