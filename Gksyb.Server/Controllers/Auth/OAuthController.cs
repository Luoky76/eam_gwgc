#pragma warning disable CA1822 // 将成员标记为 static 会使路由不可访问
using Azure;
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
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Gksyb.Server.Controllers.Auth
{
    [GksybAuthorize(true)]
    public class OAuthController : BaseController
    {
        private readonly LogPath _logPath = new("OAuth");
        private readonly ILogger<OAuthController> _logger;
        private readonly OAuthService _service;

        public OAuthController(OAuthService service, ILogger<OAuthController> logger)
        {
            _service = service;
            _logger = logger;
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
        public async Task<AjaxResult> AccessTokenAsync(string json)
        {
            string ip = null;
            string response = null;
            try
            {
                ip = Request.GetRealIP();
                var request = json.ToObject<OAuthRequest<string>>();
                request.Init(Request);
                var token = await _service.AccessTokenAsync(request);
                response = token.ToJson();
                return AjaxResult.Success(token);
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                throw;
            }
            finally
            {
                _logger.LogInformation(_logPath, $"接到来自{ip}的【AccessToken】请求，请求参数：{json},应答数据：{response}");
            }
        }

        [JsToken]
        public async Task<AjaxResult> GenerateTokenAsync([FromServices] UserSession user, string userType)
        {
            var key = userType switch
            {
                "1" => user.UserName,
                "2" => (await _service.GetUserAsync()).Phone,
                "3" => (await _service.GetUserAsync()).ToMiniJson(),
                "4" => (await _service.GetUserAsync(user.UserName, true)).ToMiniJson(),
                _ => string.IsNullOrWhiteSpace(user.WorkerCode) ? user.UserName : user.WorkerCode,
            };
            var token = await _service.TokenAsync(new TokenRequest()
            {
                Key = key,
                IP = Request.GetRealIP(),
                UA = Request.GetUserAgent()
            });
            return AjaxResult.Success(token, default);
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
                var ip = Request.GetRealIP();
                var ua = Request.GetUserAgent();
                var name = await _service.UserInfoAsync(new TokenRequest()
                {
                    Key = ticket,
                    IP = ip,
                    UA = ua
                });
                var user = name.Contains('{') ? name.ToObject<UserInfoResponse>() : await _service.GetUserAsync(name);
                var request = new LoginRequest()
                {
                    Username = user.Account,
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
        public async Task<AjaxResult> TokenAsync(string json)
        {
            string ip = null;
            string response = null;
            try
            {
                ip = Request.GetRealIP();
                var request = json.ToObject<OAuthRequest<TokenRequest>>();
                await request.Check(HttpContext);
                if (string.IsNullOrWhiteSpace(request.Data.Key))
                {
                    response = "请传递字段Key";
                    return AjaxResult.Error(response);
                }
                response = await _service.TokenAsync(request.Data);
                return AjaxResult.Success(response, default);
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                throw;
            }
            finally
            {
                _logger.LogInformation(_logPath, $"接到来自{ip}的【Token】请求，请求参数：{json},应答数据：{response}");
            }
        }

        [AllowAnonymous]
        public async Task<AjaxResult> UserInfoAsync(string json)
        {
            string ip = null;
            string response = null;
            try
            {
                ip = Request.GetRealIP();
                var request = json.ToObject<OAuthRequest<TokenRequest>>();
                await request.Check(HttpContext);
                response = await _service.UserInfoAsync(request.Data);
                return AjaxResult.Success(response, default);
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                throw;
            }
            finally
            {
                _logger.LogInformation(_logPath, $"接到来自{ip}的【UserInfo】请求，请求参数：{json},应答数据：{response}");
            }
        }
    }
}
#pragma warning restore CA1822 // 将成员标记为 static 会使路由不可访问