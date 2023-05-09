using System.Reflection;
using EventHandler = Gksyb.Common.EventBus.EventHandler;

namespace Gksyb.Common.Weixin
{
    /// <summary>
    /// 微信消息订阅
    /// </summary>
    public interface IMsgSubscriber : IBaseService
    {
        /*
         * // 事件处理程序定义规范
         * [MsgSubscriber(MsgType="text")]
         * public Task Handler(ActionData data)
         * {
         *     // To Do...
         * }
         */
    }

    public static class MsgSubscriberStore
    {
        /// <summary>
        /// 消息处理程序集合
        /// </summary>
        public static HashSet<EventHandler> MsgHandlers { get; set; } = new();

        public static void AddMsgSubscriber(this Assembly source)
        {
            var bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            source.GetExportedTypes().Where(t => t.IsPublic && t.IsClass && !t.IsInterface && !t.IsAbstract && typeof(IMsgSubscriber).IsAssignableFrom(t)).ForEach(c =>//动态注册
            {
                // 查找所有公开且贴有 [EventSubscribe] 的实例方法
                var methods = c.GetMethods(bindingAttr).Where(u => u.IsDefined(typeof(MsgSubscriberAttribute), false));
                // 遍历所有事件订阅者处理方法
                foreach (var method in methods)
                {
                    // 处理同一个事件处理程序支持多个事件 Id 情况
                    var attributes = method.GetCustomAttributes<MsgSubscriberAttribute>(false);
                    // 添加到 HashSet 集合中
                    foreach (var attribute in attributes)
                    {
                        MsgHandlers.Add(new EventHandler()
                        {
                            EventId = $"{attribute.MsgType}-{attribute.Event}".ToLower(),
                            Handler = method
                        });
                    }
                }
            });
        }
    }
}