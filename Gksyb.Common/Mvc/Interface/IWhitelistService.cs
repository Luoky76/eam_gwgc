namespace Gksyb.Common.Mvc.Interface
{
    public interface IWhitelistService : IService
    {
        /// <summary>
        /// 获取IP白名单配置
        /// </summary>
        /// <param name="appid"></param>
        /// <returns></returns>
        Task<string> GetAsync(string appid);
    }
}