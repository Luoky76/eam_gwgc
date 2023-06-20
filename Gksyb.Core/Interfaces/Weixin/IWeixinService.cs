namespace Gksyb.Core.Interfaces.Weixin
{
    public interface IWeixinService : IService
    {
        /// <summary>
        /// 创建微信通知
        /// </summary>
        /// <returns></returns>
        Task CreateNotice(WeixinNoticeRequest request, bool newDb = false);

        /// <summary>
        /// 解绑
        /// </summary>
        /// <param name="openid">微信ID或者绑定名</param>
        /// <returns></returns>
        Task UnBind(string openid);
    }
}