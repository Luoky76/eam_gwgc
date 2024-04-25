using Gksyb.Common.Quartz.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Listener;

namespace Gksyb.Common.Quartz
{
    /// <summary>
    /// 单例注入 全局处理job
    /// </summary>
    public class JobListener : JobListenerSupport
    {
        private readonly LogPath _logPath = new(nameof(JobListener));
        private readonly ILogger<JobListener> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public JobListener(ILogger<JobListener> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public override string Name => "任务监听";

        public override async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            if (context.JobDetail.JobDataMap["QuartzTask"] is QuartzTask quartzTask) quartzTask.IsExcuted = false;
            await Task.CompletedTask;
        }

        public override async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
        {
            try
            {
                if (context.JobDetail.JobDataMap["QuartzTask"] is not QuartzTask quartzTask) return;
                quartzTask.LastRunTime = context.FireTimeUtc.ToLocalTime().DateTime;
                quartzTask.ElapsedTime = context.JobRunTime.TotalMilliseconds.CastTo<int>();
                if (!quartzTask.IsExcuted)
                {
                    _logger.LogInformation(_logPath, $"任务未执行：{quartzTask.ToMiniJson()}");
                    return;
                }
                if (jobException == null)
                {
                    quartzTask.RunStatus = "正常";
                    quartzTask.LastRunResult = context.Result?.ToString();
                }
                else
                {
                    Exception ex = jobException;
                    for (var i = 0; i < 2; i++)
                    {
                        if (ex.InnerException != null)
                        {
                            ex = ex.InnerException;
                        }
                    }
                    quartzTask.RunStatus = "异常";
                    quartzTask.LastRunResult = ex.ToString();
                }
                using var scope = _serviceScopeFactory.CreateAsyncScope();
                var quartzStore = scope.ServiceProvider.GetService<IQuartzStore>();
                await quartzStore.SetTaskInfo(quartzTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(_logPath, ex.ToString());
            }
        }
    }
}