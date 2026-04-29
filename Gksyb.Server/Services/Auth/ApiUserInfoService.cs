using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Auth.Dtos;
using Gksyb.Model.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Gksyb.Server.Services.Auth
{
    public class ApiUserInfoService : IApiUserInfoService
    {
        private const string KEY = OAuthRequest<object>.KEY;
        private readonly IDbContext _dbContext;

        public ApiUserInfoService(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task FromRequestAsync(HttpRequest request, UserSession user)
        {
            if (request.Headers.TryGetValue(IApiUserInfoService.HeaderKey, out StringValues value))
            {
                if (value == "undefined")
                    return;
                var info = await GetApiUserInfoAsync(user.UserName, value);
                await SetUserSessionAsync(info, user);
            }
        }

        /// <summary>
        /// 获取应用信息
        /// </summary>
        private async Task<ApiUserInfo> GetApiUserInfoAsync(string appId, string value)
        {
            var key = await GetAppKeyAsync(appId);
            string json;
            try
            {
                json = CryptographyHelper.DecryptSM4(value, key);
            }
            catch (Exception)
            {
                key = await RefreshAppKeyAsync(appId);
                json = CryptographyHelper.DecryptSM4(value, key);
            }
            return json.ToObject<ApiUserInfo>();
        }

        /// <summary>
        /// 附加信息写入
        /// </summary>
        /// <param name="info"></param>
        /// <param name="user"></param>
        private static async Task SetUserSessionAsync(ApiUserInfo info, UserSession user)
        {
            user.RealName = info.RealName;
            user.Class = info.Class;
            user.WorkerCode = info.WorkerCode;
            await Task.CompletedTask;
        }

        /// <summary>
        /// 获取应用信息
        /// </summary>
        private async Task<string> GetAppKeyAsync(string appId)
        {
            if (Keys.TryGetValue(appId, out var appInfo))
            {
                return appInfo;
            }
            return await RefreshAppKeyAsync(appId);
        }

        /// <summary>
        /// 刷新应用信息
        /// </summary>
        private async Task<string> RefreshAppKeyAsync(string appId)
        {
            var secret = await _dbContext.Query<SYS_OAUTH>().Where(c => c.APPID == appId).Select(c => c.SECRET).FirstOrDefaultAsync();
            MessageException.ThrowIf(string.IsNullOrWhiteSpace(secret), $"未找到应用编码:{appId}的配置");
            secret = CryptographyHelper.DecryptSM4(secret, KEY);
            Keys[appId] = secret;
            return secret;
        }

        /// <summary>
        /// 密钥信息
        /// </summary>
        private static readonly Dictionary<string, string> Keys = new();
    }
}