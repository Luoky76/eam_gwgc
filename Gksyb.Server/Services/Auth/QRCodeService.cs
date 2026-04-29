using Gksyb.Core.Auth;
using Gksyb.Model.Core;
using Gksyb.Server.Controllers.Auth.Dtos;
using Microsoft.Extensions.Caching.Distributed;

namespace Gksyb.Server.Services.Auth
{
    public class QRCodeService : IBaseService
    {
        private const int SHORT_EXPIRATION = 2 * 60;
        private const int TOKEN_EXPIRATION = 5 * 60;
        private readonly IDbContext _dbContext;
        private readonly IDistributedCache _distributedCache;

        public QRCodeService(IDbContext dbContext, IDistributedCache distributedCache)
        {
            _dbContext = dbContext;
            _distributedCache = distributedCache;
        }

        public async Task<string> GenerateAsync(TokenRequest request)
        {
            var token = Guid.NewGuid().ToString("N");
            await _distributedCache.SetAsync(token, request, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(TOKEN_EXPIRATION)
            });
            return token;
        }

        public async Task<int> CheckAsync(TokenRequest request)
        {
            var token = await _distributedCache.GetAsync<TokenRequest>(request.Key);
            return await CheckTokenRequestAsync(token, request);
        }

        public async Task ConfirmAsync(string key, string info)
        {
            var token = await _distributedCache.GetAsync<TokenRequest>(key);
            MessageException.ThrowIf(token == null, "失效码，请重扫");
            token.Key = info;
            await _distributedCache.SetAsync(key, token, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(SHORT_EXPIRATION)
            });
        }

        public async Task<string> UserInfoAsync(TokenRequest request)
        {
            var token = await _distributedCache.GetAsync<TokenRequest>(request.Key);
            var status = await CheckTokenRequestAsync(token, request);
            MessageException.ThrowIf(status != 1, $"验证失败：{status}");
            return token.Key;
        }

        public async Task<string> AuthorizeUrlAsync(string token, UserSession user)
        {
            var url = await _dbContext.Query<BC_CODE>().Where(c => c.CODE_TYPE == "QRCode" && c.CODE_EN == "AuthorizeUrl").Select(c => c.CODE_CN).FirstOrDefaultAsync();
            url = string.IsNullOrWhiteSpace(url) ? "oauth/qr-confirm.html?token={token}" : url;
            url = url.Replace(null, new Dictionary<string, object>()
            {
                { "token",token},
                { "appid",user?.UserName},
                { "appname",user?.RealName}
            });
            return url;
        }

        private async Task<int> CheckTokenRequestAsync(TokenRequest token, TokenRequest request)
        {
            if (token == null)
            {
                return 1001;
            }
            if (token.IP != request.IP)
            {
                await _distributedCache.RemoveAsync(request.Key);
                return 1002;
            }
            if (token.UA != request.UA)
            {
                await _distributedCache.RemoveAsync(request.Key);
                return 1003;
            }
            return string.IsNullOrWhiteSpace(token.Key) ? 0 : 1;
        }
    }
}
