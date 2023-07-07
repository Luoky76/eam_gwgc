using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.Dtos;
using Gksyb.Model.Grid;
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

        [AllowAnonymous]
        public async Task<AjaxResult> AccessTokenAsync(OAuthRequest<string> request)
        {
            request.Init(Request);
            var user = await _service.AccessTokenAsync(request);
            return AjaxResult.Success(user.Token, "成功");
        }

        [JsToken]
        public async Task<AjaxResult> TokenAsync()
        {
            var token = await _service.TokenAsync();
            return AjaxResult.Success(token, default);
        }

        [GksybAuthorize(IsApi = true)]
        public async Task<AjaxResult> TicketAsync([FromHeader, Required] string name)
        {
            var ticket = await _service.TicketAsync(name);
            return AjaxResult.Success(ticket, default);
        }

        [AllowAnonymous]
        public async Task<string> JsTokenAsync()
        {
            return await HttpContext.GenerateTokenAsync("oauth/validTicket");
        }

        [JsToken, AllowAnonymous]
        public async Task<AjaxResult> ValidTicketAsync([FromServices] IDistributedCache distributedCache, [FromServices] IAuthService service, [FromHeader, Required] string ticket)
        {
            try
            {
                var userName = await distributedCache.GetStringAsync(ticket);
                if (string.IsNullOrWhiteSpace(userName)) return AjaxResult.Error("票据过期");
                var request = new LoginRequest()
                {
                    Username = userName,
                    IP = Request.GetRealIP(),
                    UserAgent = Request.GetUserAgent()
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