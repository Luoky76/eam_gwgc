using Gksyb.Common.Static;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using System.Collections.Concurrent;

namespace Gksyb.Server.Job
{
    /// <summary>
    /// 站内消息处理
    /// </summary>
    public class SysMessageJob : BaseJob
    {
        private readonly IDbContext _dbContext;
        private IHubContext<BroadcastChannelHub, IBroadcastChannelClient> _hubContext;

        public SysMessageJob(IDbContext dbContext, ILogger<SysMessageJob> logger) : base(logger, nameof(SysMessageJob))
        {
            _dbContext = dbContext;
        }

        public override async Task Excute()
        {
            _dbContext.DisableSqlLog();
            var sysdate = await _dbContext.GetSysdate(false);
            var listNotice = await _dbContext.Query<SYS_MESSAGE>().Where(c => c.MSG_STATUS == SYS_MESSAGE.InitStatus && c.CREATEDATE > sysdate.Value.AddMinutes(-3) && c.CREATEDATE < sysdate)
                .WhereIfNotNullOrEmpty(_quartzTask.TaskData, c => c.APPNAME == _quartzTask.TaskData)
                .Ignore(c => new { c.CREATEUSERID, c.CREATEUSER, c.CREATEDATE }).ToListAsync();
            if (listNotice.Count < 1) return;
            _logger.LogInformation(_logPath, $"准备发送站内消息数：{listNotice.Count}");
            listNotice = listNotice.OrderBy(c => c.ID).ToList();
            _hubContext = HttpContext.RequestServices.GetService<IHubContext<BroadcastChannelHub, IBroadcastChannelClient>>();
            var list = new ConcurrentBag<SYS_MESSAGE>();
            await Parallel.ForEachAsync(listNotice, async (notice, token) =>//多线程处理提高效率
            {
                try
                {
                    var isUser = string.IsNullOrWhiteSpace(notice.NOTICE_GROUP);
                    var isAutoReaded = notice.AUTO_READED == "1";
                    await _hubContext.Clients.SendAsync(ToMessageInfo(notice));
                    notice.MSG_STATUS = "1";
                    notice.SENDDATE = sysdate;
                    if (isAutoReaded)
                    {
                        notice.READDATE = sysdate;
                    }
                    list.Add(notice);
                }
                catch (Exception ex)
                {
                    _logger.LogError(_logPath, $"异常：{ex}");
                }
            });
            await list.ForEachAsync(async notice =>
            {
                await _dbContext.UpdateAsync<SYS_MESSAGE>(c => c.ID == notice.ID, c => new SYS_MESSAGE()
                {
                    MSG_STATUS = notice.MSG_STATUS,
                    SENDDATE = notice.SENDDATE,
                    READDATE = notice.READDATE
                });
            });
        }

        /// <summary>
        /// 转成messageInfo
        /// </summary>
        private static MessageInfo ToMessageInfo(SYS_MESSAGE notice) => new()
        {
            Title = notice.MSG_TITLE,
            Content = notice.MSG_CONTENT,
            DialogMode = notice.DIALOG_MODE,
            DialogType = notice.DIALOG_TYPE,
            Href = notice.MSG_HREF,
            Target = notice.MSG_HREF_TARGET,
            MobileHref = notice.MSG_MOBILE_HREF,
            Key = notice.MSG_KEY,
            Receives = string.IsNullOrWhiteSpace(notice.NOTICE_USER) ? null : new List<string>() { notice.NOTICE_USER },
            Groups = string.IsNullOrWhiteSpace(notice.NOTICE_GROUP) ? null : new List<string>() { notice.NOTICE_GROUP }
        };
    }
}