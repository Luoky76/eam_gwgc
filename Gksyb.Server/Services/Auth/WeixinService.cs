#pragma warning disable CA1822 // 将成员标记为 static

using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.Dtos;
using Microsoft.Extensions.Options;

namespace Gksyb.Server.Services.Auth
{
    public class WeixinService : WeixinBaseService, IBaseService
    {
        private readonly IAuthService _authService;
        private static readonly string _guest = "WXGUEST";

        public WeixinService(IDbContext dbContext, IAuthService authService, IOptions<SysContextOptions> sysContext) : base(dbContext, sysContext)
        {
            _authService = authService;
        }

        /// <summary>
        /// 微信单点登录
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> OauthAsync(LoginRequest request)
        {
            var openid = request.Username;
            var userPort = await _dbContext.Query<CF_USER_PORT>()
                .Where(c => c.CORPID == openid && c.APPNAME == _options.UserAppName && c.OPTYPE == _opertype).FirstOrDefaultAsync();
            userPort ??= new CF_USER_PORT() { LOGINNAME = _guest };
            var loginName = userPort.LOGINNAME;
            var user = await _dbContext.Query<CF_USER>()
                .Where(c => c.LOGINNAME == loginName && c.APPNAME == _options.UserAppName).FirstOrDefaultAsync();
            if (user == null) return AjaxResult.Error("-1");
            request.Username = user.LOGINNAME;
            request.Password = user.LOGINPASSWORD;
            request.MenuAppname = _options.MobileAppName;
            request.Source = "微信登录";
            var result = await _authService.LoginAsync(request, userSession =>
            {
                userSession.Openid = openid;
            }, false);
            if (result.IsError) return result;
            var userResponse = result.Data as UserResponse;
            userResponse.Openid = openid;
            return result;
        }

        /// <summary>
        /// 获取绑定状态
        /// </summary>
        /// <returns></returns>
        public async Task<string> BindingStaus(string openid)
        {
            //if (string.IsNullOrWhiteSpace(openid)) throw new MessageException("无法获取微信号,请退出后重试");
            await RemoveInvalid(openid);
            var userPort = await _dbContext.Query<CF_USER_PORT>()
                .Where(c => c.CORPID == openid && c.APPNAME == _options.UserAppName && c.OPTYPE == _opertype).FirstOrDefaultAsync();
            if (userPort != null) return userPort.LOGINNAME;
            //var user = await _dbContext.Query<CF_USER>()
            //    .Where(c => c.QQ == openid && c.APPNAME == _options.UserAppName).FirstOrDefaultAsync();
            //if (user == null) return "";
            //if (user.FLAG == "1") return user.LOGINNAME;
            //_dbContext.TrackEntity(user);
            //user.QQ = null;
            //await _dbContext.UpdateAsync(user);
            return "";
        }

        /// <summary>
        /// 微信绑定
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> Bind(LoginRequest request)
        {
            var openid = request.Verifycode;
            if (await BindHandle(request))
            {
                request.Username = (request.Username ?? "").ToUpper();
                request.PasswordHandle();
                var user = await _authService.GetUserAsync(request.Username, request.Password);
                if (user == null || user.LOGINPASSWORD != request.Password) return AjaxResult.Error("用户名密码错误，无法绑定");
                request.Username = user.LOGINNAME;
            }
            await RemoveInvalid(openid);
            var userPort = await _dbContext.Query<CF_USER_PORT>()
                .Where(c => c.CORPID == openid && c.APPNAME == _options.UserAppName && c.OPTYPE == _opertype).FirstOrDefaultAsync();
            if (userPort != null) return AjaxResult.Error($"此微信号已绑定{userPort.LOGINNAME}");
            var isBind = await _dbContext.Query<CF_USER_PORT>()
                .Where(c => c.LOGINNAME == request.Username && c.APPNAME == _options.UserAppName && c.OPTYPE == _opertype).AnyAsync();
            if (isBind) return AjaxResult.Error($"用户{request.Username}已绑定微信");
            var sysdate = await _dbContext.GetSysdate();
            await _dbContext.InsertAsync(new CF_USER_PORT()
            {
                LOGINNAME = request.Username,
                OPTYPE = _opertype,
                CORPID = openid,
                REMARK = request.UserAgent,
                APPNAME = _options.UserAppName,
                CREATEUSER = request.Username,
                CREATEDATE = sysdate,
                MODIFYDATE = sysdate,
                MODIFYUSER = request.Username
            });
            //if (!string.IsNullOrWhiteSpace(user.QQ)) return AjaxResult.Error($"账户{request.Username}已绑定过，请先解绑");
            //_dbContext.TrackEntity(user);
            //user.QQ = openid;
            //await _dbContext.UpdateAsync(user);
            return AjaxResult.Success();
        }

        /// <summary>
        /// 绑定处理
        /// </summary>
        /// <returns></returns>
        private async Task<bool> BindHandle(LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.MenuAppname)) return true;
            await Task.CompletedTask;
            return false;
        }

        private async Task RemoveInvalid(string openid)
        {
            await _dbContext.DeleteAsync<CF_USER_PORT>(
                   c => c.CORPID == openid && c.APPNAME == _options.UserAppName && c.OPTYPE == _opertype
                   && _dbContext.Query<CF_USER>().Where(a =>
                   a.LOGINNAME == c.LOGINNAME && a.APPNAME == _options.UserAppName && a.FLAG == "0").Any());
        }
    }
}