using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Gksyb.Server.Services.Auth
{
    public class OAuthService : IBaseService
    {
        private const int ShortExpiration = 30;
        private const string KEY = "eokW6j8@DZfwFBMiIa7ghzELcKYSuyAR";
        private static readonly string _opertype = "用户公司";
        private readonly IDbContext _dbContext;
        private readonly IDistributedCache _distributedCache;
        private readonly UserSession _user;
        private readonly SysContextOptions _options;

        public OAuthService(IDbContext dbContext, IDistributedCache distributedCache, UserSession userSession, IOptions<SysContextOptions> sysContext)
        {
            _dbContext = dbContext;
            _distributedCache = distributedCache;
            _user = userSession;
            _options = sysContext.Value;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var data = await _dbContext.Query<SYS_OAUTH>().GetGridData(request);
            var rows = data.Rows as IList<SYS_OAUTH>;
            rows.ForEach(c => c.SECRET = CryptographyHelper.DecryptSM4(c.SECRET, KEY));
            return data;
        }

        /// <summary>
        /// 保存
        /// </summary>
        public async Task<AjaxResult> SaveAsync(SaveRequest<SYS_OAUTH> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new { c.APPID, c.SECRET, c.NAME, c.IP, c.EXPIRES, c.FLAG, c.INFORMATION },
                c => a => a.ID == c.ID,
                BeforeAdd, BeforeUpdate);
        }

        /// <summary>
        /// 新增前
        /// </summary>
        private async Task BeforeAdd(SYS_OAUTH entity)
        {
            await Handle(entity);
            entity.ID = GuidHelper.NewSnowflakeId();
            entity.SECRET = CryptographyHelper.EncryptSM4(entity.SECRET, KEY);
        }

        /// <summary>
        /// 更新前
        /// </summary>
        private async Task BeforeUpdate(SYS_OAUTH entity)
        {
            await Handle(entity);
            entity.SECRET = CryptographyHelper.EncryptSM4(entity.SECRET, KEY);
        }

        /// <summary>
        /// 检查和预处理
        /// </summary>
        private async Task Handle(SYS_OAUTH entity)
        {
            var isExists = await _dbContext.Query<SYS_OAUTH>().Where(c => c.APPID == entity.APPID)
                .WhereIfNotNull(entity.ID, c => c.ID != entity.ID).AnyAsync();
            if (isExists) throw new MessageException($"已经存在编码{entity.APPID}");
        }

        public async Task<UserSession> AccessTokenAsync(OAuthRequest<string> request)
        {
            var model = await Check(request);
            var userSession = new UserSession()
            {
                Token = Guid.NewGuid().ToString("N"),
                UserID = model.ID.Value,
                UserName = model.APPID,
                RealName = model.NAME,
                IsApi = true,
                Group = model.INFORMATION,
                Roles = new List<string>(),
                IP = request.IP,
                UserAgent = request.UA,
                UserAppName = _options.UserAppName,
                RoleAppName = _options.RoleAppName,
                MenuAppname = _options.AppName,
                ForbinMenus = new List<MenuModule>(),
                ForbinButtons = new SortedList<string, List<ButtonModule>>()
            };
            var key = CryptographyHelper.GetSM3($"{model.APPID}{model.IP}{nameof(AccessTokenAsync)}");
            var lastToken = await _distributedCache.GetAsync<string>(key);
            if (!string.IsNullOrWhiteSpace(lastToken)) await _distributedCache.RemoveAsync(lastToken);
            var expiration = TimeSpan.FromSeconds(model.EXPIRES ?? 7200);
            await _distributedCache.SetAsync(key, userSession.Token, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = expiration
            });
            await _distributedCache.SetAsync(userSession.Token, userSession, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = expiration
            });
            return userSession;
        }

        /// <summary>
        /// 生成token
        /// </summary>
        public async Task<string> TokenAsync()
        {
            var token = Guid.NewGuid().ToString("N");
            await _distributedCache.SetStringAsync(token, _user.UserName, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ShortExpiration)
            });
            return token;
        }

        /// <summary>
        /// 获取ticket
        /// </summary>
        public async Task<string> TicketAsync(string name)
        {
            var query = _dbContext.Query<CF_USER>();
            if (name.IsMobileNumber())
            {
                query = query.Where(c => c.PHONE == name);
            }
            else
            {
                query = query.Where(c => c.LOGINNAME == name || c.DEPARTCODE == name);
            }
            var list = await query.Where(c => c.APPNAME == _options.UserAppName && c.FLAG == "1").Select(UserInfoResponse.FromCfUser).ToListAsync();
            MessageException.ThrowIf(list.Count < 1, $"找不到用户{name}");
            var model = list.FirstOrDefault(c => c.Phone == name) ?? list.FirstOrDefault(c => c.WorkerCode == name) ?? list.FirstOrDefault();
            var ticket = Guid.NewGuid().ToString("N");
            await _distributedCache.SetStringAsync(ticket, model.Account, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ShortExpiration)
            });
            return ticket;
        }

        /// <summary>
        /// token换取用户信息
        /// </summary>
        /// <returns></returns>
        public async Task<UserInfoResponse> UserInfoAsync(string ticketCode)
        {
            try
            {
                var userName = await _distributedCache.GetStringAsync(ticketCode);
                if (string.IsNullOrWhiteSpace(userName)) throw new MessageException("token已失效");
                var userInfo = await UserInfoResponseAsync(userName);
                await CorpInfoResponseAsync(userInfo, userInfo.CorpID);
                return userInfo;
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(ticketCode)) await _distributedCache.RemoveAsync(ticketCode);
            }
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        private async Task<UserInfoResponse> UserInfoResponseAsync(string userName)
        {
            var userInfo = await _dbContext.Query<CF_USER>()
                 .Where(c => c.LOGINNAME == userName && c.APPNAME == _options.UserAppName && c.FLAG == "1").Select(UserInfoResponse.FromCfUser).FirstOrDefaultAsync()
                 ?? throw new MessageException($"找不到用户{userName}");
            return userInfo;
        }

        /// <summary>
        /// 获取公司数据
        /// </summary>
        public async Task CorpInfoResponseAsync(UserInfoResponse user, string corpid)
        {
            user.Corp = null;
            user.AllCorp = new List<CorpInfo>();
            var userCorps = await _dbContext.Query<CF_USER_PORT>().Where(c => c.LOGINNAME == user.Account && c.APPNAME == _options.UserAppName && c.OPTYPE == _opertype).ToListAsync();
            if (userCorps.Count < 1) return;
            var allCorps = await _dbContext.Query<CF_CORP>().Where(c => c.VALIDFLAG == "1").Select(CorpInfoExtensions.SelectCorpInfo).ToListAsync();
            var corps = allCorps.Where(c => userCorps.Exists(a => a.CORPID == c.CorpID)).Select(a =>
            {
                a.Station = (userCorps.Find(c => c.CORPID == a.CorpID)?.REMARK ?? "").Split(",").DistinctAndOrderBy().ToList();
                return a;
            }).ToList();
            corps.ForEach(c =>
            {
                user.AllCorp.AddRange(c.ChildCorp(allCorps));
            });
            user.AllCorp = user.AllCorp.DistinctBy(c => c.CorpID).OrderBy(c => c.CorpID).ToList();
            if (corps == null || corps.Count < 1) return;
            user.Corp = corps.FirstOrDefault(c => c.CorpID == corpid) ?? corps[0];
        }

        /// <summary>
        /// 检查
        /// </summary>
        public async Task<SYS_OAUTH> Check(OAuthRequest<string> request)
        {
            var model = await _dbContext.Query<SYS_OAUTH>().Where(c => c.APPID == request.AppId && c.FLAG == "1").FirstOrDefaultAsync();
            MessageException.ThrowIf(model == null, $"找不到{request.AppId}的记录");
            request.Check(model.SECRET, model.IP);
            return model;
        }
    }
}