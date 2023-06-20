using Gksyb.Common;
using Gksyb.Common.Quartz;
using Gksyb.Common.Quartz.Dtos;
using Gksyb.Common.Static;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz.Spi;
using System.Text.RegularExpressions;
using System.Threading;

namespace Quartz
{
    internal class GksybTaskHostedService : BackgroundService
    {
        private readonly LogPath _logPath = new("Quartz");
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly ITypeLoadHelper _typeLoadHelper;
        private readonly ILogger<GksybTaskHostedService> _logger;
        private IScheduler scheduler = null;

        public GksybTaskHostedService(ISchedulerFactory schedulerFactory, ITypeLoadHelper typeLoadHelper, ILogger<GksybTaskHostedService> logger)
        {
            _schedulerFactory = schedulerFactory;
            _typeLoadHelper = typeLoadHelper;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.Delay(3 * 1000, stoppingToken);//延时3s
                using var scope = HttpContext.RequestServices.CreateAsyncScope();
                var quartzStore = scope.ServiceProvider.GetService<IQuartzStore>();
                var tasks = await quartzStore.GetTasks();
                tasks ??= new List<QuartzTask>();
                var addressList = HttpContext.AddressList;
                _logger.LogInformation(_logPath, $"本机IP：{addressList.ToStr(",")}");
                scheduler = await _schedulerFactory.GetScheduler(stoppingToken);
                foreach (var task in tasks)
                {
                    try
                    {
                        var ips = (task.TaskIP ?? "").Split(",").DistinctAndOrderBy().ToList();
                        if (ips.Count > 0 && !addressList.Any(ip => ips.Any(reg => Regex.IsMatch(ip, reg)))) continue;
                        Type jobType = _typeLoadHelper.LoadType(task.TaskMethod);
                        IJobDetail job = JobBuilder.Create(jobType).WithIdentity(task.TaskName, task.TaskGroup).WithDescription(task.TaskDesc)
                            .SetJobData(new JobDataMap() { { "QuartzTask", task } })
                            .Build();
                        ITrigger trigger = TriggerBuilder.Create()
                           .WithIdentity(task.TaskName, task.TaskGroup)
                           .WithDescription(task.TaskDesc)
                           .WithCronSchedule(task.TaskCron)
                           .Build();
                        await scheduler.ScheduleJob(job, trigger, stoppingToken);
                    }
                    catch (Exception e)
                    {
                        task.RunStatus = "异常";
                        task.LastRunResult = e.ToString();
                        await quartzStore.SetTaskInfo(task);
                        _logger.LogError(_logPath, $"{e}");
                    }
                }
                await scheduler.Start(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(_logPath, $"{ex}");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
            try
            {
                if (scheduler == null) return;
                await scheduler.Shutdown(true, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(_logPath, $"{ex}");
            }
        }
    }
}