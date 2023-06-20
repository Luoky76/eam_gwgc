using Gksyb.Common.Quartz.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Listener;
using System.Threading;

namespace Gksyb.Common.Quartz
{
    public class JobListener : JobListenerSupport
    {
        private readonly LogPath _logPath = new(nameof(JobListener));
        private readonly ILogger<JobListener> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private QuartzTask _quartzTask = null;

        public JobListener(ILogger<JobListener> logger, IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public override string Name => "任务监听";

        public override async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
        {
            _quartzTask = (context.JobDetail.JobDataMap["QuartzTask"] as QuartzTask) ?? new QuartzTask()
            {
                TaskID = GuidHelper.NewSnowflakeId()
            };
            _quartzTask.IsExcuted = false;
            await Task.CompletedTask;
        }

        public override async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!_quartzTask.IsExcuted) return;
                _quartzTask.LastRunTime = Convert.ToDateTime(context.FireTimeUtc.ToString());
                _quartzTask.ElapsedTime = context.JobRunTime.TotalMilliseconds.CastTo<int>();
                if (jobException == null)
                {
                    _quartzTask.RunStatus = "正常";
                    _quartzTask.LastRunResult = context.Result?.ToString();
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
                    _quartzTask.RunStatus = "异常";
                    _quartzTask.LastRunResult = ex.ToString();
                }
                using var scope = _serviceScopeFactory.CreateAsyncScope();
                var quartzStore = scope.ServiceProvider.GetService<IQuartzStore>();
                await quartzStore.SetTaskInfo(_quartzTask);
            }
            catch (Exception ex)
            {
                _logger.LogError(_logPath, ex.ToString());
            }
        }
    }
}