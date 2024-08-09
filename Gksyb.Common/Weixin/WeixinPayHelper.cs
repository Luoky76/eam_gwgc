using Flurl.Http;
using Flurl.Http.Content;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System.Xml;

namespace Gksyb.Common.Weixin
{
    public static class WeixinPayHelper
    {
        internal const string ApiHost = "https://api.mch.weixin.qq.com";//微信支付api地址

        /// <summary>
        /// 统一支付接口
        /// 统一支付接口，可接受JSAPI/NATIVE/APP 下预支付订单，返回预支付订单号。NATIVE 支付返回二维码code_url。
        /// </summary>
        /// <returns></returns>
        public static async Task<WeixinTransactionsResponse> UnifiedOrder(WeixinTransactionsRequest transactionsRequest, Action<UnifiedOrderRequest> action = null)
        {
            var request = new UnifiedOrderRequest(WeixinSetting.AppId, WeixinSetting.Mchid, transactionsRequest)
            {
                NonceStr = Guid.NewGuid().ToString("N").ToLower()
            };
            action?.Invoke(request);
            request.ComputeSign(WeixinSetting.PayKey);
            var xml = XMLHelper.Serialize(request);
            var url = $"{ApiHost}/pay/unifiedorder";
            xml = await ApiRequest(url).PostStringAsync(xml).ReceiveString();
            var response = XMLHelper.Deserialize<UnifiedOrderResponse>(xml);
            if (response.IsError) throw new MessageException(response.ToString());
            var result = WeixinTransactionsResponse.GetInstance(response.PrepayId, request.NonceStr, request.SignType);
            return result;
        }

        public static async Task<TransactionsStatusResponse> UnifiedOrderCallback(this HttpContext source)
        {
            var xml = await source.Request.GetContent();
            if (!VerifySignXml(xml))
            {
                return TransactionsStatusResponse.Error("签名验证失败");
            }
            return TransactionsStatusResponse.FromXml(xml);
        }

        /// <summary>
        /// JSAPI下单 微信不认null，所以序列化用 ToMiniJson
        /// </summary>
        /// <returns></returns>
        public static async Task<WeixinTransactionsResponse> Transactions(WeixinTransactionsRequest transactionsRequest, Action<JsApiTransactionsRequest> action = null)
        {
            var request = new JsApiTransactionsRequest(WeixinSetting.AppId, WeixinSetting.Mchid, transactionsRequest);
            action?.Invoke(request);
            var url = $"{ApiHost}/v3/pay/transactions/jsapi";
            var response = await ApiRequest(url).PostAsync(new CapturedJsonContent(request.ToMiniJson())).ReceiveJson<JsApiTransactionsResponse>();
            if (response.IsError) throw new MessageException(response.ToString());
            var nonceStr = Guid.NewGuid().ToString("N").ToLower();
            return WeixinTransactionsResponse.GetInstance(response.PrepayId, nonceStr, "RSA");
        }

        /// <summary>
        /// 订单查询
        /// </summary>
        /// <returns></returns>
        public static async Task<TransactionsStatusResponse> TransactionsQuery(string outTradeNo)
        {
            var url = $"{ApiHost}/v3/pay/transactions/out-trade-no/{outTradeNo}?mchid={WeixinSetting.Mchid}";
            var result = await ApiRequest(url).GetStringAsync();
            var response = result.ToObject<TransactionsStatusResponse>(new JsonSerializerSettings().Custom("yyyy-MM-ddTHH:mm:sszzz"));
            return response;
        }

        /// <summary>
        /// 订单关闭
        /// </summary>
        /// <returns></returns>
        public static async Task<AjaxResult> TransactionsClose(string outTradeNo)
        {
            var url = $"{ApiHost}/v3/pay/transactions/out-trade-no/{outTradeNo}/close";
            var result = await ApiRequest(url).PostJsonAsync(new { mchid = WeixinSetting.Mchid });
            if (result.StatusCode == 204) return AjaxResult.Success();
            return AjaxResult.Error($"错误码：{result.StatusCode}");
        }

        /// <summary>
        /// xml验证
        /// </summary>
        /// <returns></returns>
        private static bool VerifySignXml(string content)
        {
            var xmlDoc = new XmlDocument()
            {
                XmlResolver = null
            };
            xmlDoc.LoadXml(content);
            var childs = xmlDoc.SelectSingleNode("xml").ChildNodes;
            var list = new List<string>();
            var sign = string.Empty;
            foreach (XmlNode xmlNode in childs)
            {
                var name = xmlNode.Name;
                var value = xmlNode.InnerText;
                if (name == "sign")
                {
                    sign = value;
                    continue;
                }
                if (string.IsNullOrWhiteSpace(value)) continue;
                list.Add($"{xmlNode.Name}={value}");
            }
            list.Sort();
            list.Add($"key={WeixinSetting.PayKey}");
            var compare = CryptographyHelper.GetMd5(list.ToStr("&")).ToUpper();
            return sign == compare;
        }

        /// <summary>
        /// 支付请求封装 https://pay.weixin.qq.com/wiki/doc/apiv3/wechatpay/wechatpay4_0.shtml
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private static IFlurlRequest ApiRequest(string url)
        {
            var request = new FlurlRequest(url);
            request.Headers.Add(HeaderNames.Accept, "application/json");
            request.Headers.Add(HeaderNames.UserAgent, "jsapi");
            request.Settings.BeforeCall = call =>
            {
                var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                string nonce = Path.GetRandomFileName();
                string content = $"{call.HttpRequestMessage.Method.Method}\n{call.HttpRequestMessage.RequestUri.PathAndQuery}\n{timestamp}\n{nonce}\n{call.RequestBody ?? ""}\n";
                string signature = CryptographyHelper.RSASign(content, WeixinSetting.PayPrivateKey);
                var authorization = $"WECHATPAY2-SHA256-RSA2048 mchid=\"{WeixinSetting.Mchid}\",nonce_str=\"{nonce}\",timestamp=\"{timestamp}\",serial_no=\"{WeixinSetting.PaySerialNumber}\",signature=\"{signature}\"";
                request.Headers.Add(HeaderNames.Authorization, authorization);
            };
            return request;
        }
    }
}