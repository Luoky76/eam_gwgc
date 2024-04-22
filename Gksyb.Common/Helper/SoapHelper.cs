using Flurl.Http;
using Microsoft.Net.Http.Headers;
using System.Xml;

namespace Gksyb.Common
{
    /// <summary>
    /// soap帮助类
    /// </summary>
    public static class SoapHelper
    {
        /// <summary>
        /// 请求数据（配合soapui工具，获取request的请求体raw，分析出SOAPAction和返回值的xpath路径）
        /// </summary>
        /// <param name="url">请求url</param>
        /// <param name="action">SOAPAction请求头(直接wsdl文件查找)</param>
        /// <param name="body">soap请求体</param>
        /// <param name="xpath">xpath路径</param>
        public static async Task<string> PostAsync(string url, string action, string body, string xpath)
        {
            var response = await url.WithHeader(HeaderNames.ContentType, @"text/xml;charset=UTF-8")
            .WithHeader("SOAPAction", action)
            .PostStringAsync(body).ReceiveString();
            var doc = new XmlDocument();
            doc.LoadXml(response);
            var node = doc.SelectSingleNode($"//*[local-name()='{xpath}']");
            return node.InnerXml;
        }
    }
}