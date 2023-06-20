using Gksyb.Common;

namespace Microsoft.AspNetCore.SignalR
{
    /// <summary>
    /// 前端js监听事件
    /// </summary>
    public interface IBroadcastChannelClient
    {
        Task Excute(ActionData actionData);
    }

    /// <summary>
    ///扩展
    /// </summary>
    public static class BroadcastChannelClientExtension
    {
        /// <summary>
        /// 发送消息给调用者
        /// </summary>
        /// <returns></returns>
        public static Task SendMessage(this IHubCallerClients<IBroadcastChannelClient> source, ActionData actionData)
        {
            return source.Caller.Excute(actionData);
        }

        /// <summary>
        /// 发送消息给用户
        /// </summary>
        /// <returns></returns>
        public static Task SendMessageToUser(this IHubClients<IBroadcastChannelClient> source, string user, ActionData actionData)
        {
            return source.User(user).Excute(actionData);
        }

        /// <summary>
        /// 发送消息给多个用户
        /// </summary>
        /// <returns></returns>
        public static Task SendMessageToUsers(this IHubClients<IBroadcastChannelClient> source, IReadOnlyList<string> users, ActionData actionData)
        {
            return source.Users(users).Excute(actionData);
        }

        /// <summary>
        /// 发送消息给所有人
        /// </summary>
        /// <returns></returns>
        public static Task SendMessageToAll(this IHubClients<IBroadcastChannelClient> source, ActionData actionData)
        {
            return source.All.Excute(actionData);
        }

        /// <summary>
        /// 发送消息给组
        /// </summary>
        /// <returns></returns>
        public static Task SendMessageToGroup(this IHubClients<IBroadcastChannelClient> source, string group, ActionData actionData)
        {
            return source.Group(group).Excute(actionData);
        }

        /// <summary>
        /// 发送消息给组
        /// </summary>
        /// <returns></returns>
        public static Task SendMessageToGroups(this IHubClients<IBroadcastChannelClient> source, IReadOnlyList<string> groupNames, ActionData actionData)
        {
            return source.Groups(groupNames).Excute(actionData);
        }

        /// <summary>
        /// 错误
        /// </summary>
        /// <returns></returns>
        public static Task Error(this IHubCallerClients<IBroadcastChannelClient> source, string message)
        {
            return source.Caller.Excute(new ActionData<string>
            {
                Action = "Error",
                Data = message
            });
        }
    }
}