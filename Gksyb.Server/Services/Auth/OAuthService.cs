using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Server.Controllers.Auth.Dtos;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;

namespace Gksyb.Server.Services.Auth
{
    public class OAuthService : IBaseService
    {
        private const int ShortExpiration = 30;
        private const string KEY = OAuthRequest<object>.KEY;
        private const string _opertype = "用户公司";
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

        public async Task<AccessTokenResponse> AccessTokenAsync(OAuthRequest<string> request)
        {
            var model = await request.Check(_dbContext);
            var userSession = new UserSession()
            {
                Token = Guid.NewGuid().ToString("N"),
                Version = _options.TicketVersion,
                UserID = model.ID.Value,
                UserName = model.APPID,
                RealName = model.NAME,
                IsApi = true,
                Group = model.INFORMATION ?? "",
                AllRoles = new List<string>(),
                IP = request.IP,
                UserAgent = request.UA,
                UserAppName = _options.UserAppName,
                RoleAppName = _options.RoleAppName,
                MenuAppname = _options.AppName
            };
            var key = CryptographyHelper.GetSM3($"{model.APPID}{model.IP}{nameof(AccessTokenAsync)}");
            var lastToken = await _distributedCache.GetAsync<string>(key);
            if (!string.IsNullOrWhiteSpace(lastToken)) await _distributedCache.RemoveAsync(lastToken);
            var response = new AccessTokenResponse()
            {
                AccessToken = userSession.Token,
                ExpiresIn = model.EXPIRES ?? 7200
            };
            var expiration = TimeSpan.FromSeconds(response.ExpiresIn);
            await _distributedCache.SetAsync(key, userSession.Token, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = expiration
            });
            await _distributedCache.SetAsync(userSession.Token, userSession, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = expiration
            });
            return response;
        }

        /// <summary>
        /// 生成单点凭据
        /// </summary>
        public async Task<string> TokenAsync(TokenRequest request)
        {
            var token = Guid.NewGuid().ToString("N");
            await _distributedCache.SetAsync(token, request, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(ShortExpiration)
            });
            return token;
        }

        /// <summary>
        /// token换取用户信息
        /// </summary>
        /// <returns></returns>
        public async Task<string> UserInfoAsync(TokenRequest request)
        {
            try
            {
                var token = await _distributedCache.GetAsync<TokenRequest>(request.Key);
                MessageException.ThrowIf(string.IsNullOrWhiteSpace(token?.Key), "验证失败：1001");
                var error = string.Empty;
                var hasIp = !string.IsNullOrWhiteSpace(token.IP);
                var hasUA = !string.IsNullOrWhiteSpace(token.UA);
                if (!hasIp && !hasUA) return token.Key;
                var times = 0;
                if (!string.IsNullOrWhiteSpace(token.IP))
                {
                    if (token.IP == request.IP)
                    {
                        times += 1;
                    }
                    else
                    {
                        error = "验证失败：1002";
                    }
                }
                if (!string.IsNullOrWhiteSpace(token.UA))
                {
                    if (token.UA == request.UA)
                    {
                        times += 1;
                    }
                    else
                    {
                        error = "验证失败：1003";
                    }
                }
                MessageException.ThrowIf(times < 1, error);
                return token.Key;
            }
            finally
            {
                if (!string.IsNullOrWhiteSpace(request.Key)) await _distributedCache.RemoveAsync(request.Key);
            }
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        public async Task<UserInfoResponse> GetUserAsync(string userName = null, bool hasCorp = false)
        {
            userName ??= _user.UserName;
            var userInfo = await _dbContext.Query<CF_USER>()
                 .Where(c => (c.LOGINNAME == userName || c.PHONE == userName || c.DEPARTCODE == userName) && c.APPNAME == _options.UserAppName && c.FLAG == "1").Select(UserInfoResponse.FromCfUser).FirstOrDefaultAsync()
                 ?? throw new MessageException($"找不到用户{userName}");
            if (hasCorp) await GetCorpInfoAsync(userInfo, userInfo.CorpID);
            return userInfo;
        }

        /// <summary>
        /// 获取公司数据
        /// </summary>
        public async Task GetCorpInfoAsync(UserInfoResponse user, string corpid)
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
    }
}