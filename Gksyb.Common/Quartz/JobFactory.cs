using Gksyb.Common.Quartz.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Simpl;
using Quartz.Spi;

namespace Gksyb.Common.Quartz
{
    public class JobFactory : MicrosoftDependencyInjectionJobFactory
    {
        private readonly LogPath _logPath = new(nameof(JobListener));
        private readonly ILogger<JobListener> _logger;
        private readonly IServiceProvider _serviceProvider;

        public JobFactory(ILogger<JobListener> logger, IServiceProvider serviceProvider, IOptions<QuartzOptions> options) : base(serviceProvider, options)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public override IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            IJob job = null;
            try
            {
                try
                {
                    job = base.NewJob(bundle, scheduler);
                }
                catch (Exception e)
                {
                    if (bundle.JobDetail.JobDataMap["QuartzTask"] is QuartzTask task)
                    {
                        task.RunStatus = "“Ï≥£";
                        task.LastRunResult = e.ToString();
                        using var scope = _serviceProvider.CreateAsyncScope();
                        var quartzStore = scope.ServiceProvider.GetService<IQuartzStore>();
                        quartzStore.SetTaskInfo(task).Result();
                    }
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(_logPath, $"{ex}");
            }
            return job;
        }
    }
}