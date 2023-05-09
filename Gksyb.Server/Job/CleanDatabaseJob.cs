using Gksyb.Model.Core;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Gksyb.Server.Job
{
    /// <summary>
    /// 定时清理数据库
    /// </summary>
    public class CleanDatabaseJob : BaseJob
    {
        private readonly IDbContext _dbContext;
        private readonly DateTime _excuteDate = DateTime.Now;

        public CleanDatabaseJob(IDbContext dbContext, ILogger<CleanDatabaseJob> logger) : base(logger, nameof(CleanDatabaseJob))
        {
            _dbContext = dbContext;
        }

        public override async Task Excute()
        {
            _dbContext.DisableSqlLog();
            _dbContext.Session.CommandTimeout = 10 * 60;
            var days = _quartzTask.TaskData.CastTo(6 * 30) * -1;
            await DeleteSysMessageAsync(days);
            await DeleteSysLogAsync(days);
        }

        /// <summary>
        /// 清理系统日志
        /// </summary>
        /// <returns></returns>
        private async Task DeleteSysLogAsync(int days)
        {
            var now = await _dbContext.GetSysdate();
            var retain = now.Value.AddDays(days);
            var date = await _dbContext.Query<SYS_LOG>().MinAsync(c => c.LOGDATE) ?? now.Value;
            while (date < retain)
            {
                var next = date.AddDays(1);
                await _dbContext.DeleteAsync<SYS_LOG>(c => c.LOGDATE >= date && c.LOGDATE <= next);
                date = next;
                if (_excuteDate.AddHours(4) < DateTime.Now) return;
                await Task.Delay(500);
            }
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