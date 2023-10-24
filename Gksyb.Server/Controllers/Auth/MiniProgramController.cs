#pragma warning disable CA1822 // 将成员标记为 static 会使路由不可访问
using Gksyb.Common.Weixin;
using Gksyb.Core.Auth;
using Gksyb.Model.Dtos;
using Gksyb.Server.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;

namespace Gksyb.Server.Controllers.Auth
{
    /// <summary>
    /// 小程序
    /// </summary>
    [AllowAnonymous]
    public class MiniProgramController : ControllerBase
    {
        private readonly MiniProgramService _service;
        private readonly IConfiguration _configuration;

        public MiniProgramController(MiniProgramService service, IConfiguration configuration)
        {
            _service = service;
            _configuration = configuration;
        }

        /// <summary>
        /// 小程序单点登录
        /// </summary>
        public async Task<AjaxResult> OAuth([FromHeader] string code)
        {
            var openid = _configuration.GetValue($"{OptionName.MiniProgram}:Openid", defaultValue: "");
            var token = new SessionResponse() { ErrCode = 0, Openid = openid };
            if (string.IsNullOrWhiteSpace(openid))
            {
                token = await MiniProgramHelper.GetSession(code);
            }
            if (token.IsError) return AjaxResult.Error(token.ToString());
            return await OAuthInnerAsync(token.Openid, token.SessionKey);
        }

        /// <summary>
        /// 手机号一键登录
        /// </summary>
        [GksybAuthorize(true)]
        public async Task<AjaxResult> OAuthPhone([FromServices] UserSession user, [FromServices] IDistributedCache distributedCache, [FromHeader] string code)
        {
            if (user == null) return AjaxResult.Error("无法获取微信id，请重新进入后再试。");
            var info = await MiniProgramHelper.GetUserPhone(code);
            if (info.IsError) return AjaxResult.Error(info.ToString());
            var phone = info.Data.PhoneInfo.PhoneNumber;
            await _service.PhoneBindAsync(phone, user.Openid, Request.GetUserAgent());
            await distributedCache.RemoveAsync(user.Token);
            return await OAuthInnerAsync(user.Openid, user.ExtendData["SessionKey"]);
        }

        /// <summary>
        /// 小程序单点登录
        /// </summary>
        private async Task<AjaxResult> OAuthInnerAsync(string openid, object sessionKey)
        {
            var appname = _configuration.GetValue($"{OptionName.MiniProgram}:AppName", defaultValue: "");
            var result = await _service.OauthAsync(new LoginRequest()
            {
                Username = openid,
                IP = Request.GetRealIP(),
                UserAgent = Request.GetUserAgent(),
                MenuAppname = appname
            }, userSession =>
            {
                userSession.ExtendData["SessionKey"] = sessionKey;
            });
            if (result.IsError) result.Data = openid;
            var userResponse = result.Data as UserResponse;
            userResponse.Ticket = null;
            return result;
        }
    }
}
#pragma warning restore CA1822 // 将成员标记为 static 会使路由不可访问