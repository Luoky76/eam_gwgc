#pragma warning disable CA1822 // 将成员标记为 static 会使路由不可访问
using Gksyb.Common.Weixin;
using Gksyb.Core.Auth;
using Gksyb.Model.Dtos;
using Gksyb.Server.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Gksyb.Server.Controllers.Auth
{
    /// <summary>
    /// 验证服务
    /// </summary>
    [AllowAnonymous]
    public class WeixinController : ControllerBase
    {
        private readonly WeixinService _service;

        public WeixinController(WeixinService service)
        {
            _service = service;
        }

        [Route("[action]")]
        [HttpGet]
        public string Weixin(WeixinRequest request)
        {
            if (!request.Check()) return "error";
            return request.Echostr;
        }

        [Route("[action]")]
        public async Task<string> WeixinAsync(WeixinRequest request)
        {
            try
            {
                if (!request.Check()) return "";
                request.Content = await Request.GetContent();
                var messageBaseRequest = MessageBaseRequest.FromXml(request.Content);
                messageBaseRequest.Openid = request.Openid;
                messageBaseRequest.Excute().Wait(2000);//最多等待2秒
                return messageBaseRequest.Response;
            }
            catch (Exception)
            {
                return "success";
            }
        }

        /// <summary>
        /// 获取微信授权地址
        /// </summary>
        /// <returns></returns>
        public AjaxResult AuthorizeUrl([FromHeader] string redirectUrl)
        {
            return AjaxResult.Success(WeixinHelper.GetAuthorizeUrl(redirectUrl), "成功");
        }

        /// <summary>
        /// 获取微信id
        /// </summary>
        /// <returns></returns>
        [GksybAuthorize(true)]
        public async Task<AjaxResult> Openid()
        {
            var user = await HttpContext.GetCurrentUserAsync();
            if (user != null) return AjaxResult.Success(user.Openid);
            return AjaxResult.Error("无法获取微信id，请关闭微信再试。");
        }

        /// <summary>
        /// 获取JSSDK
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> JsSDK(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                url = Request.Headers[HeaderNames.Referer];
                url = string.IsNullOrWhiteSpace(url) ? Request.GetRealUrl() : url;
                var uri = new Uri(url);
                if (uri.Query.HasValue())
                {
                    url = url.Replace(uri.Query, "");
                }
            }
            var response = await WeixinJsSDKHelper.GetJsSdk(url);
            return AjaxResult.Success(response);
        }

        /// <summary>
        /// 微信单点登录
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> OAuth([FromHeader] string code)
        {
            var token = await WeixinHelper.GetOauthAccessToken(code);
            if (token.IsError) return AjaxResult.Error(token.ToString());
            var result = await _service.OauthAsync(new LoginRequest()
            {
                Username = token.Openid,
                IP = Request.GetRealIP(),
                UserAgent = Request.GetUserAgent()
            });
            if (result.IsError) result.Data = token.Openid;
            return result;
        }

        /// <summary>
        /// 获取微信绑定状态
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> BindingStaus(string openid)
        {
            var user = await HttpContext.GetCurrentUserAsync();
            if (user != null) openid = user.Openid;
            if (string.IsNullOrWhiteSpace(openid)) return AjaxResult.Error("无法获取微信号,请退出后重试");
            var name = await _service.BindingStaus(openid);
            return AjaxResult.Success(string.IsNullOrWhiteSpace(name) ? "0" : "1", name);
        }

        /// <summary>
        /// 微信绑定
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> Bind(LoginRequest request)
        {
            var user = await HttpContext.GetCurrentUserAsync();
            if (user != null) request.Verifycode = user.Openid;
            if (string.IsNullOrWhiteSpace(request.Verifycode)) return AjaxResult.Error("无法获取微信号,请退出后重试");
            var result = await _service.Bind(request);
            return result;
        }

        /// <summary>
        /// 微信解绑
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> UnBind(string openid)
        {
            var user = await HttpContext.GetCurrentUserAsync();
            if (user != null) openid = user.Openid;
            if (string.IsNullOrWhiteSpace(openid)) return AjaxResult.Error("无法获取微信号,请退出后重试");
            await _service.UnBind(openid);
            return AjaxResult.Success();
        }
    }
}
#pragma warning restore CA1822 // 将成员标记为 static 会使路由不可访问