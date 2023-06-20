namespace Gksyb.Common.Weixin
{
    /// <summary>
    /// 微信AccessToken处理封装
    /// </summary>
    public interface IAccessTokenHandle
    {
        /// <summary>
        /// 获取AccessToken
        /// </summary>
        Task<string> GetAsync();

        /// <summary>
        /// 设置AccessToken
        /// </summary>
        Task SetAsync(AccessTokenResponse accessToken);
    }
}