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
        /// 处理Href
        /// </summary>
        public static void BuildHref(this MessageInfo source)
        {
            var isHrefMatch = Regex.IsMatch(source.Href ?? "", @"{(\w+)}");
            var isMobileMatch = Regex.IsMatch(source.MobileHref ?? "", @"{(\w+)}");
            if (!isHrefMatch && !isMobileMatch) return;
            Dictionary<string, object> dic = null;
            try
            {
                dic = source.Data?.ToJson().ToObject<Dictionary<string, object>>();
            }
            catch
            {
            }
            dic ??= new Dictionary<string, object>();
            dic.Add("Key", source.Key);
            source.Href = isHrefMatch ? source.Href.Replace(null, dic) : source.Href;
            source.MobileHref = isHrefMatch ? source.MobileHref.Replace(null, dic) : source.MobileHref;
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