using System.Reflection;

namespace Gksyb.Common.EventBus
{
    /// <summary>
    /// 事件订阅
    /// </summary>
    public interface IEventSubscriber : IBaseService
    {
        /*
         * // 事件处理程序定义规范
         * [EventSubscribe(EventID)]
         * public Task Handler(ActionData data)
         * {
         *     // To Do...
         * }
         */
    }

    public static class IEventSubscriberExtension
    {
        /// <summary>
        /// 批量注册事件订阅者
        /// </summary>
        public static void AddIEventSubscriber(this Assembly source)
        {
            var bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            source.GetExportedTypes().Where(t => t.IsPublic && t.IsClass && !t.IsInterface && !t.IsAbstract && typeof(IEventSubscriber).IsAssignableFrom(t)).ForEach(c =>//动态注册
            {
                // 查找所有公开且贴有 [EventSubscribe] 的实例方法
                var methods = c.GetMethods(bindingAttr).Where(u => u.IsDefined(typeof(EventSubscribeAttribute), false));
                // 遍历所有事件订阅者处理方法
                foreach (var method in methods)
                {
                    // 处理同一个事件处理程序支持多个事件 Id 情况
                    var attributes = method.GetCustomAttributes<EventSubscribeAttribute>(false);
                    // 添加到 HashSet 集合中
                    foreach (var attribute in attributes)
                    {
                        EventBusStore.EventHandlers.Add(new EventHandler()
                        {
                            EventId = attribute.EventId,
                            Handler = method
                        });
                    }
                }
            });
        }
    }
}