using Gksyb.Model.Core;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Gksyb.Server.Job
{
    /// <summary>
    /// 定时清理消息
    /// </summary>
    public class CleanMessageJob : BaseJob
    {
        private readonly IDbContext _dbContext;
        private readonly DateTime _excuteDate = DateTime.Now;

        public CleanMessageJob(IDbContext dbContext, ILogger<CleanMessageJob> logger) : base(logger, nameof(CleanMessageJob))
        {
            _dbContext = dbContext;
        }

        public override async Task Excute()
        {
            _dbContext.DisableSqlLog();
            _dbContext.Session.CommandTimeout = 10 * 60;
            var days = _quartzTask.TaskData.CastTo(12 * 31) * -1;
            await DeleteSysMessageAsync(days);
        }

        /// <summary>
        /// 清理系统消息
        /// </summary>
        /// <returns></returns>
        private async Task DeleteSysMessageAsync(int days)
        {
            var now = await _dbContext.GetSysdate();
            var retain = now.Value.AddDays(days);
            var date = await _dbContext.Query<SYS_MESSAGE>().MinAsync(c => c.CREATEDATE) ?? now.Value;
            while (date < retain)
            {
                var next = date.AddDays(1);
                await _dbContext.DeleteAsync<SYS_MESSAGE>(c => c.CREATEDATE >= date && c.CREATEDATE <= next);
                date = next;
                if (_excuteDate.AddHours(4) < DateTime.Now) return;
                await Task.Delay(500);
            }
        }
    }
}