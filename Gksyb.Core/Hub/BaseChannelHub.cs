using Gksyb.Core.Auth;

namespace Microsoft.AspNetCore.SignalR
{
    /// <summary>
    /// 通道接口
    /// </summary>
    [GksybAuthorize(true)]
    public class BaseChannelHub : Hub<IBroadcastChannelClient>, IBaseService
    {
    }
}