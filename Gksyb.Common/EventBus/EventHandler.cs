using System.Reflection;

namespace Gksyb.Common.EventBus
{
    public class EventHandler
    {
        /// <summary>
        /// 事件 Id
        /// </summary>
        public string EventId { get; set; }

        /// <summary>
        /// 事件处理程序
        /// </summary>
        public MethodInfo Handler { get; set; }
    }
}