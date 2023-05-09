using Gksyb.Common.Quartz.Dtos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Quartz;
using Quartz.Simpl;
using Quartz.Spi;

namespace Gksyb.Common.Quartz
{
    public class JobFactory : MicrosoftDependencyInjectionJobFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public JobFactory(IServiceProvider serviceProvider, IOptions<QuartzOptions> options) : base(serviceProvider, options)
        {
            _serviceProvider = serviceProvider;
        }

        public override IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            IJob job = null;
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
            }
            return job;
        }
    }
}