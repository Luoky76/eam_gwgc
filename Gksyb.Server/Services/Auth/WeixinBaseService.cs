using Gksyb.Common.Static;
using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Weixin;
using Gksyb.Model.Core;
using Microsoft.Extensions.Options;

namespace Gksyb.Server.Services.Auth
{
    public class WeixinBaseService : IWeixinService
    {
        protected readonly IDbContext _dbContext;
        protected readonly SysContextOptions _options;
        protected static readonly string _opertype = "微信";

        public WeixinBaseService(IDbContext dbContext, IOptions<SysContextOptions> sysContext)
        {
            _dbContext = dbContext;
            _options = sysContext.Value;
        }

        ///<inheritdoc/>
        public async Task UnBind(string openid)
        {
            await _dbContext.DeleteAsync<CF_USER_PORT>(c => (c.CORPID == openid || c.LOGINNAME == openid) && c.APPNAME == _options.UserAppName && c.OPTYPE == _opertype);
            await _dbContext.UpdateAsync<CF_USER>(c => (c.QQ == openid || c.LOGINNAME == openid) && c.APPNAME == _options.UserAppName, c => new CF_USER()
            {
                QQ = null
            });
        }

        /// <inheritdoc/>
        public async Task CreateNotice(WeixinNoticeRequest request, bool newDb = false)
        {
            var openids = new List<string>();
            if (string.IsNullOrWhiteSpace(request.Openid))
            {
                if (string.IsNullOrWhiteSpace(request.Receiver)) throw new MessageException("接收人或openid不能为空");
                var receives = request.Receiver.Split(',');
                openids = await _dbContext.Query<CF_USER_PORT>()
                    .Where(c => (receives.Contains(c.LOGINNAME) || receives.Contains(c.CORPID)) && c.APPNAME == _options.UserAppName && c.OPTYPE == _opertype)
                    .Select(c => c.CORPID).ToListAsync();
            }
            else
            {
                openids.Add(request.Openid);
            }
            if (string.IsNullOrWhiteSpace(request.Creater))
            {
                var user = HttpContext.Current?.GetCurrentUserAsync().Result();
                user ??= new UserSession()
                {
                    UserName = "Auto"
                };
                request.Creater = user.UserName;
            }
            var dbContext = newDb ? _dbContext.Clone() : _dbContext;
            try
            {
                var now = await _dbContext.GetSysdate();
                if (!request.SendTime.HasValue) request.SendTime = now;
                var notices = openids.Select(openid => new WEIXIN_NOTICE()
                {
                    SID = GuidHelper.NewShortId(),
                    TEMPLATE = request.Template,
                    RECEIVER = openid,
                    URL = request.Url,
                    TDATA = request.TData,
                    CREATEUSER = request.Creater,
                    CREATEDATE = request.SendTime,
                    MODIFYUSER = request.Creater,
                    MODIFYDATE = now
                }).ToList();
                await dbContext.InsertRangeAsync(notices);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (newDb) dbContext.Dispose();
            }
        }
    }
}