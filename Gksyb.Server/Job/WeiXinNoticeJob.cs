using Gksyb.Common.Weixin;
using Gksyb.Model.Core;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Gksyb.Server.Job
{
    /// <summary>
    /// 微信通知
    /// </summary>
    public class WeiXinNoticeJob : BaseJob
    {
        private readonly IDbContext _dbContext;

        public WeiXinNoticeJob(IDbContext dbContext, ILogger<WeiXinNoticeJob> logger) : base(logger, nameof(WeiXinNoticeJob))
        {
            _dbContext = dbContext;
        }

        public override async Task Excute()
        {
            _dbContext.DisableSqlLog();
            var sysdate = await _dbContext.GetSysdate(false);
            var listNotice = await _dbContext.Query<WEIXIN_NOTICE>().Where(c => c.STATUS == WEIXIN_NOTICE.InitStatus && c.CREATEDATE > sysdate.Value.AddDays(-1) && c.CREATEDATE < sysdate).ToListAsync();
            if (listNotice.Count < 1) return;
            _logger.LogInformation(_logPath, $"准备发送微信通知数：{listNotice.Count}");
            listNotice = listNotice.OrderBy(c => c.CREATEDATE).ToList();
            await Parallel.ForEachAsync(listNotice, async (notice, token) =>//多线程处理提高效率
            {
                AjaxResult result = null;
                try
                {
                    try
                    {
                        result = await WeixinHelper.SendTemplateMessage(notice.RECEIVER, notice.TEMPLATE, notice.URL, notice.TDATA);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(_logPath, $"异常：{ex}");
                        result = AjaxResult.Error(ex.ToString());
                    }
                    notice.RESULT = result.Message.SubStr(0, 200);
                    if (result.IsError)
                    {
                        notice.RETRY ??= 0;
                        notice.RETRY += 1;
                        if (notice.RETRY >= 3)
                        {
                            notice.STATUS = "失败";
                        }
                    }
                    else
                    {
                        notice.STATUS = "已发送";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(_logPath, $"异常：{ex}");
                }
            });
            await listNotice.ForEachAsync(async notice =>
            {
                await _dbContext.UpdateAsync<WEIXIN_NOTICE>(c => c.SID == notice.SID, c => new WEIXIN_NOTICE()
                {
                    RESULT = notice.RESULT,
                    SENDDATE = DateTime.Now,
                    RETRY = notice.RETRY,
                    STATUS = notice.STATUS
                });
            });
        }
    }
}