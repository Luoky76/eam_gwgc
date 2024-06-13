using Microsoft.Extensions.Caching.Distributed;

namespace Gksyb.Common.Weixin
{
    /// <summary>
    /// 小程序AccessToken处理封装
    /// </summary>
    public interface IMiniProgramAccessTokenHandle : IAccessTokenHandle
    {
    }

    /// <summary>
    /// 实现微信AccessToken处理封装
    /// </summary>
    internal class MiniProgramAccessTokenHandle : AccessTokenHandle, IMiniProgramAccessTokenHandle
    {
        public MiniProgramAccessTokenHandle(IDistributedCache distributedCache) : base(distributedCache)
        {
            AccessTokenCacheName = "MiniProgram_AccessToken";
        }
    }
}