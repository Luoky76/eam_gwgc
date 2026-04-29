#pragma warning disable CA1822 // 将成员标记为 static 会使路由不可访问
using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Core;
using Gksyb.Model.Dtos;
using Gksyb.Server.Controllers.Auth.Dtos;
using Gksyb.Server.Services.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Gksyb.Server.Controllers.Auth
{
    public class QRCodeController : BaseController
    {
        private readonly LogPath _logPath = new("QRCode");
        private readonly ILogger<QRCodeController> _logger;
        private readonly QRCodeService _service;
        private readonly IOAuthApiService _apiService;

        public QRCodeController(QRCodeService service, IOAuthApiService apiService, ILogger<QRCodeController> logger)
        {
            _service = service;
            _apiService = apiService;
            _logger = logger;
        }

        [AllowAnonymous]
        public async Task<string> JsTokenAsync()
        {
            return await HttpContext.GenerateTokenAsync("qrcode");
        }

        [JsToken("qrcode"), AllowAnonymous]
        public async Task<AjaxResult> GenerateAsync([FromHeader] string appId)
        {
            var request = new TokenRequest()
            {
                IP = Request.GetRealIP(),
                UA = Request.GetUserAgent()
            };
            if (string.IsNullOrWhiteSpace(appId))
            {
                var token = await _service.GenerateAsync(request);
                var url = await _service.AuthorizeUrlAsync(token, null);
                return AjaxResult.Success(token, url);
            }
            return await _apiService.PostJsonAsync<AjaxResult>(appId, "qrcode/authorizeUrl", request);
        }

        [AllowAnonymous]
        public async Task<AjaxResult> CheckAsync([FromHeader, Required] string token, [FromHeader] string appId)
        {
            var request = new TokenRequest()
            {
                Key = token,
                IP = Request.GetRealIP(),
                UA = Request.GetUserAgent()
            };
            if (string.IsNullOrWhiteSpace(appId))
            {
                var status = await _service.CheckAsync(request);
                return AjaxResult.Success(status);
            }
            return await _apiService.PostJsonAsync<AjaxResult>(appId, "qrcode/status", request);
        }

        [JsToken("qrcode"), AllowAnonymous]
        public async Task<AjaxResult> SSOAsync([FromServices] IAuthService service, [FromServices] OAuthService oAuthService, [FromHeader, Required] string token, [FromHeader] string appId, [FromHeader] string menuAppname)
        {
            var ip = Request.GetRealIP();
            var ua = Request.GetUserAgent();
            var tokenRequest = new TokenRequest()
            {
                Key = token,
                IP = ip,
                UA = ua
            };
            string name;
            if (string.IsNullOrWhiteSpace(appId))
            {
                name = await _service.UserInfoAsync(tokenRequest);
            }
            else
            {
                var result = await _apiService.PostJsonAsync<AjaxResult<string>>(appId, "qrcode/userinfo", tokenRequest);
                if (result.IsError)
                {
                    return result;
                }
                name = result.Data;
            }
            var user = name.Contains('{') ? name.ToObject<UserInfoResponse>() : await oAuthService.GetUserAsync(name);
            var request = new LoginRequest()
            {
                Username = user.Account,
                IP = ip,
                UserAgent = ua
            };
            request.Password = await service.GetPasswordAsync(request.Username);
            request.Source = "QRCode";
            request.MenuAppname = menuAppname;
            return await service.LoginAsync(request, checkPassword: false);
        }

        [JsToken, GksybAuthorize(true)]
        public async Task<AjaxResult> ConfirmAsync(string token, string userType, [FromServices] OAuthService service)
        {
            var info = await service.GetStoreKeyAsync(userType);
            await _service.ConfirmAsync(token, info);
            return AjaxResult.Success();
        }

        [GksybAuthorize(IsApi = true)]
        public async Task<AjaxResult> AuthorizeUrlAsync(string json, [FromServices] UserSession user)
        {
            string ip = null;
            string response = null;
            try
            {
                ip = Request.GetRealIP();
                var request = json.ToObject<TokenRequest>();
                var token = await _service.GenerateAsync(request);
                var url = await _service.AuthorizeUrlAsync(token, user);
                url = url.Replace(null, new Dictionary<string, object>()
                {
                    { "token",token},
                    { "appid",user.UserName},
                    { "appname",user.RealName}
                });
                response = $"{token},{url}";
                return AjaxResult.Success(token, url);
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                throw;
            }
            finally
            {
                _logger.LogInformation(_logPath, $"接到来自{ip}的【AuthorizeUrl】请求，请求参数：{json},应答数据：{response}");
            }
        }

        [GksybAuthorize(IsApi = true)]
        public async Task<AjaxResult> StatusAsync([FromBody] TokenRequest request)
        {
            var status = await _service.CheckAsync(request);
            return AjaxResult.Success(status);
        }

        [GksybAuthorize(IsApi = true)]
        public async Task<AjaxResult> UserInfoAsync(string json)
        {
            string ip = null;
            string response = null;
            try
            {
                ip = Request.GetRealIP();
                var request = json.ToObject<TokenRequest>();
                var name = await _service.UserInfoAsync(request);
                response = name;
                return AjaxResult.Success(name, default);
            }
            catch (Exception ex)
            {
                response = ex.ToString();
                throw;
            }
            finally
            {
                _logger.LogInformation(_logPath, $"接到来自{ip}的【UserInfoAsync】请求，请求参数：{json},应答数据：{response}");
            }
        }
    }
}
#pragma warning restore CA1822 // 将成员标记为 static 会使路由不可访问