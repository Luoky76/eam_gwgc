using Gksyb.Common.Quartz;
using Gksyb.Common.Quartz.Dtos;
using Gksyb.Model.Core;

namespace Gksyb.Server.Services.System
{
    /// <summary>
    /// 任务存储
    /// </summary>
    public class QuartzStore : IQuartzStore
    {
        private readonly IDbContext _dbContext;

        public QuartzStore(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 获取任务
        /// </summary>
        /// <returns></returns>n
        public async Task<List<QuartzTask>> GetTasks()
        {
            try
            {
                List<QuartzTask> list = null;
                await _dbContext.NotSqlLog(async () =>
                {
                    list = await _dbContext.Query<SYS_TASK>().Where(c => c.TASK_STATUS == "正常").Select(c => new QuartzTask()
                    {
                        TaskID = c.ID.Value,
                        TaskMethod = c.TASK_INVOKE,
                        TaskName = c.TASK_NAME,
                        TaskGroup = c.TASK_GROUP,
                        TaskDesc = c.TASK_DESC,
                        TaskCron = c.TASK_CRON,
                        TaskData = c.TASK_DATA,
                        TaskView = c.TASK_VIEW,
                        TaskIP = c.TASK_RUNIP,
                        TaskErrorMatch = c.TASK_ERROR_REGEX,
                        TaskErrorMethod = c.TASK_ERROR_INVOKE,
                        LastRunIP = c.TASK_LAST_RUNIP
                    }).ToListAsync();
                });
                return list;
            }
            catch (Exception)
            {
                return new List<QuartzTask>();
            }
        }

        /// <summary>
        /// 设置任务状态
        /// </summary>
        /// <returns></returns>
        public async Task SetTaskInfo(QuartzTask task)
        {
            var status = task.LastRunResult.SubStr(0, 4000, true);
            var lastRunIp = Gksyb.Common.Static.HttpContext.AddressList.ToStr(",").SubStr(0, 500, true);
            await _dbContext.NotSqlLog(async () =>
            {
                await _dbContext.UpdateAsync<SYS_TASK>(c => c.ID == task.TaskID, c => new SYS_TASK
                {
                    TASK_RUNSTATUS = task.RunStatus,
                    TASK_LAST_RUNDATE = task.LastRunTime,
                    TASK_LAST_RUNIP = lastRunIp,
                    TASK_LAST_KEY = task.LastKey,
                    TASK_ELAPSED_TIME = task.ElapsedTime,
                    TASK_LAST_RESULT = status
                });
            });
        }
    }
}