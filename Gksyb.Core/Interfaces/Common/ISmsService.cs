namespace Gksyb.Core.Interfaces.Common
{
    /// <summary>
    /// 短信服务
    /// </summary>
    public interface ISmsService : IService
    {
        /// <summary>
        /// 发送短信
        /// </summary>
        public Task SendAsync(SmsInfo info);

        /// <summary>
        /// 生成短信码
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <param name="second">验证码有效期（单位秒）</param>
        /// <param name="key">标识</param>
        /// <returns>校验码</returns>
        public Task<string> GenerateCodeAsync(string phone, int second = 120, string key = null);

        /// <summary>
        /// 校验短信码
        /// </summary>
        /// <param name="phone">手机号</param>
        /// <param name="second">验证码</param>
        /// <returns>状态码 -1 失败，0成功，>1可再重试次数</returns>
        public Task<int> CheckCodeAsync(string phone, string code);
    }
}