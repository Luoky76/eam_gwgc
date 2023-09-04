using Gksyb.Common.Quartz;
using Gksyb.Core.Interfaces.Common;

namespace Gksyb.Server.Job.NoticeHandle
{
    /// <summary>
    /// 通知
    /// </summary>
    public class NoticeHandle : IBaseService, INoticeHandle
    {
        private readonly IMessageCenterService _messageCenterService;

        public NoticeHandle(IMessageCenterService messageCenterService)
        {
            _messageCenterService = messageCenterService;
        }

        public async Task Excute(string type, string content)
        {
            MessageInfo info = null;
            try
            {
                info = content.ToObject<MessageInfo>();
            }
            catch (Exception)
            {
            }
            if (info == null || string.IsNullOrWhiteSpace(info.Code)) return;
            await _messageCenterService.SendAsync(info);
        }
    }
}