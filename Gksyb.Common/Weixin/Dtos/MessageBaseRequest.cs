using Gksyb.Common.Static;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Security.Cryptography;
using System.Xml;

namespace Gksyb.Common.Weixin
{
    /// <summary>
    /// 请求消息基础类
    /// </summary>
    public class MessageBaseRequest : MessageBase
    {
        public MessageBaseRequest()
        {
        }

        /// <summary>
        /// 消息ID
        /// </summary>
        public long? MsgId { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        public string MsgType { get; set; }

        /// <summary>
        /// 事件类型
        /// </summary>
        public string Event { get; set; }

        /// <summary>
        /// 消息体
        /// </summary>
        public XmlElement MsgBody { get; set; }

        /// <summary>
        /// 返回
        /// </summary>
        public string Response { get; set; } = "success";

        /// <summary>
        /// 设置返回值
        /// </summary>
        public void SetResponse(string msgType, string append)
        {
            var createTime = (DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds.CastTo<long>();
            Response = $@"<xml><ToUserName><![CDATA[{FromUserName}]]></ToUserName><FromUserName><![CDATA[{ToUserName}]]></FromUserName><CreateTime>{createTime}</CreateTime><MsgType><![CDATA[{msgType}]]></MsgType>{append}</xml>";
        }

        public Task Excute()
        {
            var eventID = $"{MsgType}-{Event}".ToLower();
            var eventHandlers = MsgSubscriberStore.MsgHandlers.Where(c => c.EventId == eventID).ToList();
            if (eventHandlers.Count < 1) return Task.CompletedTask;
            return Task.Factory.StartNew(() =>
            {
                using var scope = HttpContext.RequestServices.CreateAsyncScope();
                Parallel.ForEach(eventHandlers, eventHandler =>
                {
                    try
                    {
                        object obj = null;
                        if (!eventHandler.Handler.IsStatic)
                        {
                            obj = scope.ServiceProvider.GetService(eventHandler.Handler.DeclaringType);
                        }
                        var invokeResult = eventHandler.Handler!.Invoke(obj, new object[] { this });
                        if (invokeResult is Task task)
                        {
                            task.Result();
                        }
                    }
                    catch (Exception)
                    {
                    }
                });
            });
        }

        /// <summary>
        /// XML内容转实体
        /// </summary>
        /// <returns></returns>
        public static MessageBaseRequest FromXml(string content)
        {
            var xmlDoc = new XmlDocument()
            {
                XmlResolver = null
            };
            xmlDoc.LoadXml(content);
            return FromXml(xmlDoc);
        }

        /// <summary>
        /// XML内容转实体
        /// </summary>
        /// <returns></returns>
        public static MessageBaseRequest FromXml(XmlDocument xmlDoc)
        {
            var response = new MessageBaseRequest();
            var xmlNode = xmlDoc["xml"];
            if (xmlNode["MsgType"] == null && xmlNode["Encrypt"] != null)//加密模式
            {
                var encryptContent = Convert.FromBase64String(xmlNode["Encrypt"]?.InnerText);
                var content = CryptographyHelper.DecryptAES(encryptContent, WeixinSetting.EncodingAESKey, null, CipherMode.CBC, PaddingMode.None);
                var msgLen = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(content, 16));
                var xml = Encoding.UTF8.GetString(content, 20, msgLen);
                xmlDoc.LoadXml(xml);
                xmlNode = xmlDoc["xml"];
            }
            response.ToUserName = xmlNode["ToUserName"]?.InnerText;
            response.FromUserName = xmlNode["FromUserName"]?.InnerText;
            response.MsgType = xmlNode["MsgType"]?.InnerText;
            response.Event = xmlNode["Event"]?.InnerText;
            response.MsgId = xmlNode["MsgId"]?.InnerText?.CastTo<long?>();
            var createTime = xmlNode["CreateTime"]?.InnerText;
            response.CreateTime = string.IsNullOrWhiteSpace(createTime) ? DateTime.Now : DateTime.UnixEpoch.AddSeconds(createTime.CastTo<double>(0)).ToLocalTime();
            response.MsgBody = xmlNode;
            return response;
        }
    }
}