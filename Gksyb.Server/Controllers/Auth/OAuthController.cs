#pragma warning disable CA1822 // 将成员标记为 static 会使路由不可访问
using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.Dtos;
using Gksyb.Model.Grid;
using Gksyb.Server.Controllers.Auth.Dtos;
using Gksyb.Server.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.ComponentModel.DataAnnotations;

namespace Gksyb.Server.Controllers.Auth
{
    [GksybAuthorize(true)]
    public class OAuthController : BaseController
    {
        private readonly OAuthService _service;

        public OAuthController(OAuthService service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        [GksybAuthorize(IsSuper = true)]
        public async Task<AjaxResult<GridData>> ListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.ListAsync(request));
        }

        /// <summary>
        /// 保存
        /// </summary>
        [JsToken, GksybAuthorize(IsSuper = true)]
        public async Task<AjaxResult> SaveAsync(SaveRequest<SYS_OAUTH> request)
        {
            return await _service.SaveAsync(request);
        }

        [AllowAnonymous, HttpGet, HttpPost]
        public AjaxResult<DateTime> Now() => AjaxResult<DateTime>.Success(DateTime.Now);

        [AllowAnonymous]
        public async Task<AjaxResult> AccessTokenAsync(OAuthRequest<string> request)
        {
            request.Init(Request);
            var user = await _service.AccessTokenAsync(request);
            return AjaxResult.Success(user.Token, "成功");
        }

        [JsToken]
        public async Task<AjaxResult> GenerateTokenAsync()
        {
            var token = await _service.GenerateTokenAsync();
            return AjaxResult.Success(token, default);
        }

        [AllowAnonymous]
        public async Task<AjaxResult> TokenAsync(OAuthRequest<TokenRequest> request)
        {
            request.Init(Request);
            await _service.Check(request);
            var ticket = await _service.TicketAsync(request.Data);
            return AjaxResult.Success(ticket, default);
        }

        [GksybAuthorize(IsApi = true)]
        public async Task<AjaxResult> TicketAsync(TokenRequest request)
        {
            var ticket = await _service.TicketAsync(request);
            return AjaxResult.Success(ticket, default);
        }

        [AllowAnonymous]
        public async Task<string> JsTokenAsync()
        {
            return await HttpContext.GenerateTokenAsync($"{Request.PathBase}oauth/validTicket");
        }

        [JsToken, AllowAnonymous]
        public async Task<AjaxResult> ValidTicketAsync([FromServices] IDistributedCache distributedCache, [FromServices] IAuthService service, [FromHeader, Required] string ticket)
        {
            try
            {
                var info = await distributedCache.GetAsync<TokenRequest>(ticket);
                if (info == null) return AjaxResult.Error("验证失败：1001");
                var ip = Request.GetRealIP();
                var ua = Request.GetUserAgent();
                if (!string.IsNullOrWhiteSpace(info.IP) && info.IP != ip) return AjaxResult.Error("验证失败：1002");
                if (!string.IsNullOrWhiteSpace(info.UA) && info.UA != CryptographyHelper.GetSM3(ua)) return AjaxResult.Error("验证失败：1003");
                var request = new LoginRequest()
                {
                    Username = info.Account,
                    IP = ip,
                    UserAgent = ua
                };
                request.Password = await service.GetPasswordAsync(request.Username);
                request.Source = "Ticket";
                return await service.LoginAsync(request, checkPassword: false);
            }
            finally
            {
                await distributedCache.RemoveAsync(ticket);
            }
        }

        [AllowAnonymous]
        public async Task<AjaxResult> UserInfoAsync(OAuthRequest<string> request)
        {
            request.Init(Request);
            await _service.Check(request);
            var userInfo = await _service.UserInfoAsync(request.Data);
            return AjaxResult.Success(userInfo);
        }
    }
}
#pragma warning restore CA1822 // 将成员标记为 static 会使路由不可访问