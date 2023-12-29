#pragma warning disable CA1822 // 将成员标记为 static 会使路由不可访问
using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Dtos;
using Gksyb.Model.UI;
using Gksyb.Server.Services.System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Gksyb.Server.Controllers.Auth
{
    /// <summary>
    /// 验证服务
    /// </summary>
    [GksybAuthorize(true)]
    public partial class AuthController : BaseController
    {
        /// <summary>
        /// 设备（个人）唯一ID
        /// </summary>
        [AllowAnonymous]
        public AjaxResult IMEI() => AjaxResult.Success(Guid.NewGuid().ToString("N").ToLower(), default);

        /// <summary>
        /// 是否内部IP
        /// </summary>
        [AllowAnonymous, HttpGet]
        public AjaxResult IsInnerIP()
        {
            return AjaxResult.Success(HttpContext.Request.IsInnerIP() ? "1" : "0", "成功");
        }

        [HttpGet]
        public AjaxResult IsLogin()
        {
            return AjaxResult.Success(DateTime.Now);
        }

        /// <summary>
        /// 验证码
        /// </summary>
        [AllowAnonymous, HttpGet]
        public async Task<IActionResult> VerifyCode([FromServices] IDistributedCache distributedCache)
        {
            var uid = HttpContext.GetUID();
            var byteContent = VerifyCodeHelper.GetVerifyCode(out string verifyCode);
            await distributedCache.SetStringAsync($"{uid}-VerifyCode", verifyCode.ToUpper(), new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
            return File(byteContent.ToArray(), @"image/png");
        }

        [AllowAnonymous]
        public async Task<string> LoginTokenAsync()
        {
            return await HttpContext.GenerateTokenAsync($"{Request.PathBase}Auth/Login");
        }

        /// <summary>
        /// 获取jstoken
        /// </summary>
        public async Task<string> JsTokenAsync(string key)
        {
            return await HttpContext.GenerateTokenAsync(key);
        }

        /// <summary>
        /// 登陆
        /// </summary>
        [AllowAnonymous, JsToken]
        public async Task<AjaxResult> Login([FromServices] IDistributedCache distributedCache, [FromServices] IAuthService service, LoginRequest request)
        {
            if ("0".Equals(IsInnerIP().Data) && !await ValidVerifyCodeAsync(request.Verifycode))
                return AjaxResult.Error("请输入正确的验证码");
            request.Username = (request.Username ?? "").ToUpper();
            request.IP = Request.GetRealIP();
            request.UserAgent = Request.GetUserAgent();
            request.IMEI = CryptographyHelper.GetMd5(request.IMEI);
            var result = await distributedCache.LimitRetry($"{request.Username}_RC", "密码输错多次，请三分钟后重试", async () =>
            {
                return await service.LoginAsync(request.PasswordHandle(), handle: async result =>
                {
                    if (!result.IsAuth || (result.LastIMEI == request.IMEI) || result.Phone == null || result.Phone.Count < 1) return null;
                    return await SmsHandleAsync(distributedCache, result);
                });
            });
            return result;
        }

        /// <summary>
        /// 短信二次验证
        /// </summary>
        private async Task<AjaxResult> SmsHandleAsync(IDistributedCache distributedCache, LoginResponse result)
        {
            await distributedCache.SetAsync(result.IMEI, result, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });
            if (result.Phone.Count > 1) return AjaxResult.Success(result.Phone, "999");
            var smsService = HttpContext.RequestServices.GetService<ISmsService>();
            await smsService.GenerateCodeAsync(result.Phone.FirstOrDefault().Key);
            return AjaxResult.Success("99");
        }

        /// <summary>
        /// 短信验证
        /// </summary>
        [AllowAnonymous, JsToken("Auth/Login")]
        public async Task<AjaxResult> PhoneSelect([FromServices] IDistributedCache distributedCache, KeyValueItem request)
        {
            var imei = CryptographyHelper.GetMd5(request.Key);
            var phone = CryptographyHelper.DecryptFront(request.Value);
            var error = "超过有效期，请重新登录";
            var model = await distributedCache.GetAsync<LoginResponse>(imei);
            MessageException.ThrowIf(model == null, $"{error}状态码：1002");
            MessageException.ThrowIf(model.Phone == null || !model.Phone.Any(a => a.Key == phone), $"{error}状态码：1003");
            var item = model.Phone.FirstOrDefault(a => a.Key == phone);
            model.Phone = new List<KeyValueItem> { item };
            return await SmsHandleAsync(distributedCache, model);
        }

        /// <summary>
        /// 短信验证
        /// </summary>
        [AllowAnonymous, JsToken("Auth/Login")]
        public async Task<AjaxResult> SmsAuth([FromServices] IDistributedCache distributedCache, [FromServices] ISmsService smsService, [FromServices] IAuthService service, KeyValueItem request)
        {
            var imei = CryptographyHelper.GetMd5(request.Key);
            var code = CryptographyHelper.DecryptFront(request.Value);
            var error = "超过有效期，请重新登录。";
            var model = await distributedCache.GetAsync<LoginResponse>(imei);
            MessageException.ThrowIf(model == null, $"{error}状态码：1002");
            MessageException.ThrowIf(model.Phone == null || model.Phone.Count != 1, $"{error}状态码：1003");
            var phone = model.Phone.FirstOrDefault().Key;
            var times = await smsService.CheckCodeAsync(phone, code);
            MessageException.ThrowIf(times < 0, $"验证码已失效，请重新登录");
            if (times > 0) return AjaxResult.Success($"验证失败，剩余次数:{times}", "99");
            await service.SetUserImeiAsync(model.Account, imei);
            await distributedCache.RemoveAsync(imei);
            return AjaxResult.Success(model.Response);
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        [AllowAnonymous, JsToken("Auth/Login")]
        public async Task<AjaxResult> ChangePasswordAsync([FromServices] IDistributedCache distributedCache, [FromServices] IAuthService service, ChangePasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                request.Username = CurrentUser?.UserName;
            }
            if (string.IsNullOrWhiteSpace(request.Username)) return AjaxResult.Error("请输入用户名");
            request.Username = request.Username.ToUpper();
            return await distributedCache.LimitRetry($"{request.Username}_RC", "密码输错多次，请三分钟后重试", async () =>
            {
                return await service.ChangePasswordAsync(request);
            });
        }

        /// <summary>
        /// 退出
        /// </summary>
        [AllowAnonymous]
        public async Task<AjaxResult> Exit([FromServices] IDistributedCache distributedCache, [FromServices] IAuthService service, [FromHeader] string ticket)
        {
            if (CurrentUser != null)
            {
                await service.ExitAsync(CurrentUser);
                await distributedCache.RemoveAsync(CurrentUser.Token);
            }
            if (string.IsNullOrWhiteSpace(ticket)) return AjaxResult.Success();
            var result = await ValidTicket(ticket);
            if (result.IsError) return AjaxResult.Success();
            await distributedCache.SetStringAsync(ticket, "1", new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = result.Data.Expiration
            });
            return AjaxResult.Success();
        }

        /// <summary>
        /// 刷新Token
        /// </summary>
        [AllowAnonymous]
        public async Task<AjaxResult> RefreshToken([FromServices] IDistributedCache distributedCache, [FromServices] IAuthService service, [FromHeader] string ticket)
        {
            ticket ??= "";
            if (CurrentUser != null)
                return AjaxResult.Success(CurrentUser.ToUserResponse(ticket), "成功");
            var result = await ValidTicket(ticket);
            if (result.IsError) return result;
            var user = result.Data;
            await distributedCache.SetStringAsync(ticket, "1", new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = user.Expiration
            });
            var request = new LoginRequest()
            {
                Username = user.UserName,
                MenuAppname = user.MenuAppname,
                RoleAppname = user.RoleAppName,
                IP = Request.GetRealIP(),
                UserAgent = Request.GetUserAgent()
            };
            request.Password = await service.GetPasswordAsync(request.Username);
            request.Source = "刷新Token";
            return await service.LoginAsync(request, userSession =>
            {
                userSession.ExtendData = user.ExtendData;
            }, false);
        }

        /// <summary>
        /// 获取菜单
        /// </summary>
        public async Task<AjaxResult> MyMenus([FromServices] IAuthService service, string appname)
        {
            if (string.IsNullOrEmpty(appname)) appname = CurrentUser.MenuAppname;
            var list = await service.MyMenusAsync(CurrentUser, appname);
            return AjaxResult.Success(list);
        }

        /// <summary>
        /// 获取按钮
        /// </summary>
        public async Task<AjaxResult> MyButtons([FromServices] IAuthService service, string menuNo, string group, string prefix, string appname)
        {
            if (string.IsNullOrEmpty(appname)) appname = CurrentUser.MenuAppname;
            var list = await service.MyButtonsAsync(CurrentUser, menuNo, appname);
            if (!string.IsNullOrWhiteSpace(group))
            {
                list = list.FindAll(c => c.INITSTATUS == group);
            }
            if (prefix.HasValue())
            {
                list = list.FindAll(c => c.BTNNO.StartsWith(prefix));
            }
            return AjaxResult.Success(list);
        }

        /// <summary>
        /// 用户组织
        /// </summary>
        public async Task<AjaxResult> UserCorps([FromServices] IAuthService service)
        {
            var user = CurrentUser;
            var corps = await service.UserCorps(user);
            return AjaxResult.Success(corps, user.Corp?.CorpID);
        }

        /// <summary>
        /// 切换组织
        /// </summary>
        public async Task<AjaxResult> ChangeCorp([FromServices] IAuthService service, string corpid)
        {
            var user = CurrentUser;
            await service.ChangeCorp(user, corpid);
            await user.SaveAsync();
            return AjaxResult.Success(user.ToUserResponse(null));
        }

        /// <summary>
        /// 验证验证码
        /// </summary>
        private async Task<bool> ValidVerifyCodeAsync(string verifycode)
        {
            var distributedCache = HttpContext.RequestServices.GetService<IDistributedCache>();
            var uid = HttpContext.GetUID();
            var key = $"{uid}-VerifyCode";
            verifycode = (verifycode ?? "").ToUpper();
            var vcode = (await distributedCache.GetStringAsync(key)) ?? "";
            await distributedCache.RemoveAsync(key);
            return !string.IsNullOrEmpty(verifycode) && verifycode == vcode;
        }

        /// <summary>
        /// 验证票据
        /// </summary>
        private async Task<AjaxResult<UserSession>> ValidTicket(string ticket)
        {
            try
            {
                var options = HttpContext.RequestServices.GetService<IOptions<SysContextOptions>>();
                var user = UserSession.ParseTicket(ticket, options.Value.TicketVersion);
                MessageException.ThrowIf(UserSession.Hash(Request.GetUserAgent()) != user.UserAgent, "无效票据");
                MessageException.ThrowIf(Request.GetRealIP() != user.IP, "无效票据");
                var distributedCache = HttpContext.RequestServices.GetService<IDistributedCache>();
                MessageException.ThrowIf(await distributedCache.GetStringAsync(ticket) == "1", "无效票据");
                return AjaxResult<UserSession>.Success(user);
            }
            catch (Exception ex)
            {
                return AjaxResult<UserSession>.Error(ex.ToString());
            }
        }


        [GksybAuthorize(IsSuper = true)]
        public AjaxResult Logs(string logPath)
        {
            var logs = Serilog.Sinks.MemoryQueue.MemoryQueueSink.Logs.ToList();
            if (!string.IsNullOrWhiteSpace(logPath))
            {
                logs = logs.FindAll(c => c.Contains(logPath));
            }
            return AjaxResult.Success(logs);
        }


        [GksybAuthorize(IsSuper = true)]
        public AjaxResult Services(string search)
        {
            var services = Gksyb.Common.Static.HttpContext.ServiceCollection
                .OrderByDescending(c => c.ServiceType.FullName).Select(c => $"Lifetime = {c.Lifetime}, ServiceType = {c.ServiceType}, ImplementationType = {c.ImplementationType}");
            if (!string.IsNullOrWhiteSpace(search)) services = services.Where(c => c.Contains(search, StringComparison.OrdinalIgnoreCase));
            return AjaxResult.Success(services);
        }

        [GksybAuthorize(IsSuper = true)]
        public async Task<AjaxResult> UpdateCacheAsync([FromServices] ConfigurationService configurationService, [FromServices] IRoleModuleService roleModuleService, [FromServices] IOptions<SysContextOptions> options)
        {
            await configurationService.UpdateCacheAsync();
            await roleModuleService.Clear(options.Value.RoleAppName, CurrentUser.MenuAppname);
            await roleModuleService.Clear(options.Value.RoleAppName, options.Value.AppName);
            await roleModuleService.Clear(options.Value.RoleAppName, options.Value.MobileAppName);
            return AjaxResult.Success();
        }
    }
}
#pragma warning restore CA1822 // 将成员标记为 static 会使路由不可访问