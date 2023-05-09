#pragma warning disable CA1822 // 将成员标记为 static 会使路由不可访问
using Gksyb.Common.EventBus;
using Gksyb.Common.Quartz.Dtos;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Quartz;
using Quartz.Impl.Triggers;

namespace Gksyb.Server.Services.System
{
    /// <summary>
    /// 任务调度
    /// </summary>
    public class TaskService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly UserSession _user;
        private readonly IEventPublisher _eventPublisher;

        /// <summary>
        /// 配置服务
        /// </summary>
        public TaskService(IDbContext dbContext, UserSession userSession, IEventPublisher eventPublisher)
        {
            _dbContext = dbContext;
            _user = userSession;
            _eventPublisher = eventPublisher;
        }

        /// <summary>
        /// 获取下次处理时间
        /// </summary>
        /// <returns></returns>
        public DateTimeOffset? GetNextFireTimeUtc(string cron, DateTimeOffset? afterTimeUtc = null)
        {
            try
            {
                var trigger = new CronTriggerImpl
                {
                    CronExpressionString = cron
                };
                afterTimeUtc ??= SystemTime.UtcNow();
                return trigger.GetFireTimeAfter(afterTimeUtc);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取单行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<SYS_TASK> GetAsync(long id)
        {
            return await _dbContext.Query<SYS_TASK>().Where(c => c.ID == id).FirstAsync();
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var gridData = await _dbContext.Query<TaskResponse>().GetGridData(request);
            var rows = gridData.Rows as IList<TaskResponse>;
            foreach (var row in rows)
            {
                if (string.IsNullOrWhiteSpace(row.TASK_CRON)) continue;
                var nextTime = GetNextFireTimeUtc(row.TASK_CRON);
                if (!nextTime.HasValue) continue;
                row.NextFireTime = nextTime.Value.ToLocalTime().DateTime;
            }
            return gridData;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> Save(SaveRequest<SYS_TASK> request)
        {
            var result = await _dbContext.SaveEntityAnsyc(request,
                c => new { c.TASK_NAME, c.TASK_GROUP, c.TASK_DESC, c.TASK_CRON, c.TASK_STATUS, c.TASK_INVOKE, c.TASK_DATA, c.TASK_VIEW, c.TASK_RUNIP, c.TASK_ERROR_REGEX, c.TASK_ERROR_INVOKE },
                c => a => a.ID == c.ID
                , BeforeAdd, BeforeUpdate, BeforeDelete);
            if (result.IsError) return result;
            if (request.Deleted?.Count > 0)
            {
                await _eventPublisher.BroadcastAsync(new ActionData<List<QuartzTask>>()
                {
                    Action = "QuartzTaskDelete",
                    Data = request.Deleted.Select(c => ToQuartzTask(c)).ToList()
                });
            }
            if (request.Added?.Count > 0)
            {
                await _eventPublisher.BroadcastAsync(new ActionData<List<QuartzTask>>()
                {
                    Action = "QuartzTaskAdd",
                    Data = request.Added.Select(c => ToQuartzTask(c)).ToList()
                });
            }
            if (request.Updated?.Count > 0)
            {
                await _eventPublisher.BroadcastAsync(new ActionData<List<QuartzTask>>()
                {
                    Action = "QuartzTaskUpdate",
                    Data = request.Updated.Select(c => ToQuartzTask(c)).ToList()
                });
            }
            return result;
        }

        /// <summary>
        /// 执行一次
        /// </summary>
        /// <returns></returns>
        public async Task Excute(List<SYS_TASK> request)
        {
            await _eventPublisher.BroadcastAsync(new ActionData<List<QuartzTask>>()
            {
                Action = "QuartzTaskExcute",
                Data = request.Select(c => ToQuartzTask(c)).ToList()
            });
        }

        /// <summary>
        /// 新增前
        /// </summary>
        /// <returns></returns>
        private async Task BeforeAdd(SYS_TASK entity)
        {
            entity.TASK_NAME.CheckNotNullOrWhiteSpace("任务名称");
            if (!entity.TASK_CRON.IsValidCron())
            {
                throw new MessageException($"表达式{entity.TASK_CRON}有错");
            }
            if (await _dbContext.Query<SYS_TASK>().Where(c => c.TASK_NAME == entity.TASK_NAME).AnyAsync())
            {
                throw new MessageException($"任务{entity.TASK_NAME}已存在");
            }
            entity.TASK_DATA = CryptographyHelper.DecryptFront(entity.TASK_DATA);
            entity.TASK_ERROR_REGEX = CryptographyHelper.DecryptFront(entity.TASK_ERROR_REGEX);
            entity.ID = GuidHelper.NewSnowflakeId();
            entity.MODIFYUSER = _user.IP;
        }

        /// <summary>
        /// 更新前
        /// </summary>
        /// <returns></returns>
        private async Task BeforeUpdate(SYS_TASK entity)
        {
            entity.TASK_NAME.CheckNotNullOrWhiteSpace("任务名称");
            if (!entity.TASK_CRON.IsValidCron())
            {
                throw new MessageException($"表达式{entity.TASK_CRON}有错");
            }
            var orgin = await _dbContext.Query<SYS_TASK>().Where(c => c.ID == entity.ID).FirstAsync();
            if (orgin.TASK_NAME != entity.TASK_NAME)
            {
                if (await _dbContext.Query<SYS_TASK>().Where(c => c.TASK_NAME == entity.TASK_NAME).AnyAsync())
                {
                    throw new MessageException($"任务{entity.TASK_NAME}已存在");
                }
            }
            entity.TASK_DATA = CryptographyHelper.DecryptFront(entity.TASK_DATA);
            entity.TASK_ERROR_REGEX = CryptographyHelper.DecryptFront(entity.TASK_ERROR_REGEX);
            entity.MODIFYUSER = _user.IP;
        }

        /// <summary>
        /// 删除前
        /// </summary>
        /// <returns></returns>
        private async Task BeforeDelete(SYS_TASK entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 任务类型装欢
        /// </summary>
        /// <returns></returns>
        private static QuartzTask ToQuartzTask(SYS_TASK task)
        {
            return new QuartzTask()
            {
                TaskID = task.ID.Value,
                TaskMethod = task.TASK_INVOKE,
                TaskName = task.TASK_NAME,
                TaskGroup = task.TASK_GROUP,
                TaskDesc = task.TASK_DESC,
                TaskCron = task.TASK_CRON,
                TaskData = task.TASK_DATA,
                TaskView = task.TASK_VIEW,
                TaskIP = task.TASK_RUNIP,
                TaskErrorMatch = task.TASK_ERROR_REGEX,
                TaskErrorMethod = task.TASK_ERROR_INVOKE,
                IsStop = task.TASK_STATUS != "正常"
            };
        }
    }
}
#pragma warning restore CA1822 // 将成员标记为 static 会使路由不可访问