using Gksyb.Common.EventBus;
using Gksyb.Common.Quartz.Dtos;
using Gksyb.Common.Static;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Spi;
using System.Text.RegularExpressions;

namespace Gksyb.Server.EventSubscriber
{
    // 实现 IEventSubscriber 接口
    public class TaskSubscriber : IEventSubscriber
    {
        private readonly LogPath _logPath = new(nameof(TaskSubscriber));
        private readonly ILogger<TaskSubscriber> _logger;
        private readonly IScheduler _scheduler;
        private readonly ITypeLoadHelper _typeLoadHelper;
        private readonly List<string> _addresss;

        public TaskSubscriber(ISchedulerFactory schedulerFactory, ITypeLoadHelper typeLoadHelper, ILogger<TaskSubscriber> logger)
        {
            _logger = logger;
            _scheduler = schedulerFactory.GetScheduler().Result();
            _typeLoadHelper = typeLoadHelper;
            _addresss = HttpContext.AddressList;
        }

        [EventSubscribe("QuartzTaskAdd")]
        public async Task QuartzTaskAdd(List<QuartzTask> tasks)
        {
            foreach (var task in tasks)
            {
                await JobHandle(task);
            }
        }

        [EventSubscribe("QuartzTaskUpdate")]
        public async Task QuartzTaskUpdate(List<QuartzTask> tasks)
        {
            foreach (var task in tasks)
            {
                await JobHandle(task);
            }
        }

        [EventSubscribe("QuartzTaskDelete")]
        public async Task QuartzTaskDelete(List<QuartzTask> tasks)
        {
            foreach (var task in tasks)
            {
                await _scheduler.DeleteJob(new JobKey(task.TaskName, task.TaskGroup));
            }
        }

        [EventSubscribe("QuartzTaskExcute")]
        public async Task QuartzTaskExcute(List<QuartzTask> tasks)
        {
            foreach (var task in tasks)
            {
                await _scheduler.TriggerJob(new JobKey(task.TaskName, task.TaskGroup));
            }
        }

        private async Task JobHandle(QuartzTask task)
        {
            try
            {
                var isDelete = await _scheduler.DeleteJob(new JobKey(task.TaskName, task.TaskGroup));
                if (task.IsStop) return;
                var ips = (task.TaskIP ?? "").Split(",").DistinctAndOrderBy().ToList();
                if (ips.Count > 0 && !_addresss.Any(ip => ips.Any(reg => Regex.IsMatch(ip, reg)))) return;
                Type jobType = _typeLoadHelper.LoadType(task.TaskMethod);
                IJobDetail job = JobBuilder.Create(jobType).WithIdentity(task.TaskName, task.TaskGroup).WithDescription(task.TaskDesc)
                    .SetJobData(new JobDataMap() { { "QuartzTask", task } })
                    .Build();
                ITrigger trigger = TriggerBuilder.Create()
                   .WithIdentity(task.TaskName, task.TaskGroup)
                   .WithDescription(task.TaskDesc)
                   .WithCronSchedule(task.TaskCron)
                   .Build();
                await _scheduler.ScheduleJob(job, trigger);
            }
            catch (Exception ex)
            {
                _logger.LogError(_logPath, ex.ToString());
            }
        }
    }
}