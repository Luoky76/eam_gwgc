using Gksyb.Core.Interfaces.Auth;
using Gksyb.Model.Core;
using Microsoft.Extensions.Options;

namespace Gksyb.Server.Services.Auth
{
    public class MiniProgramService : WeixinService, IBaseService
    {
        public MiniProgramService(IDbContext dbContext, IAuthService authService, IOptions<SysContextOptions> sysContext) : base(dbContext, authService, sysContext)
        {
            _opertype = "小程序";
        }

        /// <summary>
        /// 手机号绑定处理
        /// </summary>
        public async Task PhoneBindAsync(string phone, string openid, string ua)
        {
            var user = await _dbContext.Query<CF_USER>()
                .Where(c => c.PHONE == phone && c.APPNAME == _options.UserAppName && c.FLAG == "1").FirstOrDefaultAsync();
            if (user == null) return;
            var userPort = await _dbContext.Query<CF_USER_PORT>()
                .Where(c => c.CORPID == openid && c.APPNAME == _options.UserAppName && c.OPTYPE == _opertype).FirstOrDefaultAsync();
            userPort ??= new CF_USER_PORT() { LOGINNAME = _guest };
            if (userPort.LOGINNAME == user.LOGINNAME) return;
            var loginName = user.LOGINNAME;
            await _dbContext.UseTransactionAsync(async () =>
            {
                await UnBind(openid);
                var sysdate = await _dbContext.GetSysdate();
                await _dbContext.InsertAsync(new CF_USER_PORT()
                {
                    LOGINNAME = loginName,
                    OPTYPE = _opertype,
                    CORPID = openid,
                    REMARK = ua,
                    APPNAME = _options.UserAppName,
                    CREATEUSER = loginName,
                    CREATEDATE = sysdate,
                    MODIFYDATE = sysdate,
                    MODIFYUSER = loginName
                });
            });
        }
    }
}