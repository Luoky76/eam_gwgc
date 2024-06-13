using Flurl.Http;
using Gksyb.Common.Static;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Gksyb.Common.Weixin
{
    public static class WeixinJsSDKHelper
    {
        private const string JsApiTicketCacheName = "Weixin_JsApiTicket";//微信JsApi缓存名
        private static JsApiTicketResponse _jsApiTicket;//微信JsApi

        /// <summary>
        /// 获取JsSDK
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<JsSdkResponse> GetJsSdk(string url)
        {
            var ticket = await GetJsApiTicket();
            return JsSdkResponse.GetInstance(ticket, url);
        }

        /// <summary>
        /// 获取JsApi
        /// </summary>
        /// <returns></returns>
        private static async Task<string> GetJsApiTicket()
        {
            var ticket = await GetJsApiTicketString();
            if (!string.IsNullOrWhiteSpace(ticket)) return ticket;
            await GetJsApiTicketInner();
            return _jsApiTicket?.Ticket;
        }

        /// <summary>
        /// 获取JsApi
        /// </summary>
        /// <returns></returns>
        private static async Task<string> GetJsApiTicketString()
        {
            if (_jsApiTicket != null && !_jsApiTicket.IsExpires) return _jsApiTicket.Ticket;
            await GetJsApiTicketFromCache();
            if (_jsApiTicket != null && !_jsApiTicket.IsExpires) return _jsApiTicket.Ticket;
            return null;
        }

        /// <summary>
        /// 从缓存获取JsApi
        /// </summary>
        /// <returns></returns>
        private static async Task GetJsApiTicketFromCache()
        {
            var distributedCache = HttpContext.RequestServices.GetService<IDistributedCache>();
            _jsApiTicket = await distributedCache.GetAsync<JsApiTicketResponse>(JsApiTicketCacheName);
        }

        /// <summary>
        /// 获取JsApi
        /// </summary>
        /// <returns></returns>
        private static async Task GetJsApiTicketInner()
        {
            _jsApiTicket = null;
            await WeixinHelper.ApiInvoke(async accessToken =>
            {
                await DistributedLockHelper.LockAsync($"{nameof(WeixinJsSDKHelper)}_{nameof(GetJsApiTicketInner)}", 120 * 1000, async isFail =>
                {
                    if (isFail)
                    {
                        await Task.Delay(3 * 1000);
                        await GetJsApiTicketFromCache();
                        return;
                    }
                    var url = $"{WeixinHelper.ApiHost}/cgi-bin/ticket/getticket?access_token={accessToken}&type=jsapi";
                    _jsApiTicket = await url.GetJsonAsync<JsApiTicketResponse>();
                    if (_jsApiTicket.IsError) return;
                    _jsApiTicket.SeExpiresTime();
                    var distributedCache = HttpContext.RequestServices.GetService<IDistributedCache>();
                    await distributedCache.SetAsync(JsApiTicketCacheName, _jsApiTicket, new DistributedCacheEntryOptions()
                    {
                        AbsoluteExpiration = _jsApiTicket.ExpiresTime
                    });
                });
                return _jsApiTicket;
            });
        }
    }
}