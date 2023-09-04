using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;

namespace Gksyb.Core.Interfaces.Common
{
    public static class MessageInfoExtensions
    {
        /// <summary>
        /// 站内信
        /// </summary>
        public const string Message = "Message";

        /// <summary>
        /// 微信
        /// </summary>
        public const string Weixin = "Weixin";

        /// <summary>
        /// 短信
        /// </summary>
        public const string Sms = "Sms";

        /// <summary>
        /// 处理数据
        /// </summary>
        public static void Handle(this MessageInfo source)
        {
            var dic = source.GetDicData(false);
            if (Regex.IsMatch(source.Href ?? "", @"{(\w+)}"))
            {
                source.Href = source.Href.Replace(null, dic);
            }
            if (Regex.IsMatch(source.MobileHref ?? "", @"{(\w+)}"))
            {
                source.MobileHref = source.MobileHref.Replace(null, dic);
            }
            if (Regex.IsMatch(source.Template ?? "", @"{(\w+)}"))
            {
                source.Template = source.Template.Replace(null, dic);
            }
        }

        /// <summary>
        /// 发送消息给调用者
        /// </summary>
        /// <returns></returns>
        public static async Task SendAsync(this IHubClients<IBroadcastChannelClient> source, MessageInfo info, bool isAll = false)
        {
            var action = string.IsNullOrWhiteSpace(info.Action) ? ActionName : info.Action;
            ActionData actionData = action == ActionName ? new ActionData<MessageInfoBase>()
            {
                Action = action,
                Data = info.MapTo<MessageInfoBase>()
            } : new ActionData()
            {
                Action = action,
                Data = info.Data ?? info.Content
            };
            if (isAll)
            {
                await source.SendMessageToAll(actionData);
                return;
            }
            if (info.Groups?.Count > 0)
            {
                await source.SendMessageToGroups(info.Groups, actionData);
            }
            if (info.Receives?.Count > 0)
            {
                await source.SendMessageToUsers(info.Receives, actionData);
            }
        }

        public const string ActionName = "MessageCenter";
    }
}