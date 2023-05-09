using Microsoft.Extensions.Caching.Distributed;

namespace Gksyb.Common.Weixin
{
    /// <summary>
    /// 实现微信AccessToken处理封装
    /// </summary>
    internal sealed class AccessTokenHandle : IAccessTokenHandle
    {
        private static AccessTokenResponse _accessToken;//微信AccessToken
        private static readonly string AccessTokenCacheName = "Weixin_AccessToken";//微信AccessToken缓存名
        private readonly IDistributedCache _distributedCache;

        public AccessTokenHandle(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        /// <inheritdoc/>
        public async Task<string> GetAsync()
        {
            if (_accessToken != null && !_accessToken.IsExpires) return _accessToken.AccessToken;
            _accessToken = await _distributedCache.GetAsync<AccessTokenResponse>(AccessTokenCacheName);
            if (_accessToken != null && !_accessToken.IsExpires) return _accessToken.AccessToken;
            return null;
        }

        ///<inheritdoc/>
        public async Task SetAsync(AccessTokenResponse accessToken)
        {
            _accessToken = accessToken;
            if (_accessToken == null) return;
            if (_accessToken.IsError) throw new MessageException(_accessToken.ToString());
            await _distributedCache.SetAsync(AccessTokenCacheName, accessToken, new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = _accessToken.ExpiresTime
            });
        }
    }
}