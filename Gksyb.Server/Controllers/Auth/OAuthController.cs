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
        [GksybAuthorize(IsDeveloper = true)]
        public async Task<AjaxResult<GridData>> ListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.ListAsync(request));
        }

        /// <summary>
        /// 保存
        /// </summary>
        [JsToken, GksybAuthorize(IsDeveloper = true)]
        public async Task<AjaxResult> SaveAsync(SaveRequest<SYS_OAUTH> request)
        {
            return await _service.SaveAsync(request);
        }

        [AllowAnonymous, HttpGet, HttpPost]
        public AjaxResult<DateTime> Now() => AjaxResult<DateTime>.Success(DateTime.Now);

        /// <summary>
        /// 获取调用各接口的access_token
        /// </summary>
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

        /// <summary>
        /// 本系统单点登录外系统用到的token
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> GenerateTokenAsync(string userType)
        {
            var token = await _service.TokenAsync(new TokenRequest()
            {
                Key = await _service.GetStoreKeyAsync(userType),
                IP = Request.GetRealIP(),
                UA = Request.GetUserAgent()
            });
            return AjaxResult.Success(token, default);
        }

        /// <summary>
        /// 本系统单点登录外系统用到url，url后面带token
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> SSOUrlAsync(string appid, string userType)
        {
            var url = await _service.GetSSOUrlAsync(appid);
            var token = await _service.TokenAsync(new TokenRequest()
            {
                Key = await _service.GetStoreKeyAsync(userType),
                IP = Request.GetRealIP(),
                UA = Request.GetUserAgent()
            });
            url = $"{url}{(url.Contains('?') ? "&" : "?")}token={token}";
            return AjaxResult.Success(url, default);
        }

        [AllowAnonymous]
        public async Task<string> JsTokenAsync()
        {
            return await HttpContext.GenerateTokenAsync("oauth/validTicket");
        }

        /// <summary>
        /// 外系统单点本系统，先获取外系统token然后跳转本系统的地址
        /// </summary>
        [JsToken("oauth/validTicket"), AllowAnonymous]
        public async Task<AjaxResult> SSOAsync([FromServices] IAuthService service, [FromHeader, Required] string token, [FromHeader] string menuAppname)
        {
            var ip = Request.GetRealIP();
            var ua = Request.GetUserAgent();
            var name = await _service.GetSSONameAsync(new TokenRequest()
            {
                Key = token,
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
            request.Source = "SSO";
            request.MenuAppname = menuAppname;
            return await service.LoginAsync(request, checkPassword: false);
        }

        /// <summary>
        /// 外系统单点本系统，先获取本系统token然后跳转本系统的地址
        /// </summary>
        [JsToken, AllowAnonymous]
        public async Task<AjaxResult> ValidTicketAsync([FromServices] IDistributedCache distributedCache, [FromServices] IAuthService service, [FromHeader, Required] string ticket, [FromHeader] string menuAppname)
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
                request.MenuAppname = menuAppname;
                return await service.LoginAsync(request, checkPassword: false);
            }
            finally
            {
                await distributedCache.RemoveAsync(ticket);
            }
        }

        /// <summary>
        /// 外系统单点本系统，获取token
        /// </summary>
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

        /// <summary>
        /// 本系统单点跳转外系统，外系统调用本接口获取用户信息
        /// </summary>
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