namespace Gksyb.Core.Interfaces.Common
{
    /// <summary>
    /// 消息中心
    /// </summary>
    public interface IMessageCenterService : IService
    {
        /// <summary>
        /// 发送消息
        /// </summary>
        Task SendToAllAsync(MessageInfo info);

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="info">消息</param>
        /// <param name="isCode">是否找不到Code就不发送</param>
        /// <returns></returns>
        Task SendAsync(MessageInfo info, bool isCode = false);
    }
}