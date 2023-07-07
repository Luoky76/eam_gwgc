using Gksyb.Common.Quartz.Dtos;
using Microsoft.AspNetCore.Hosting;

namespace Gksyb.Common.Quartz
{
    /// <summary>
    /// 任务存储
    /// </summary>
    public class QuartzStoreFromFile : IQuartzStore
    {
        private readonly IWebHostEnvironment _environment;
        private static List<QuartzTask> _tasks;

        public QuartzStoreFromFile(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        /// <summary>
        /// 获取任务
        /// </summary>
        /// <returns></returns>n
        public async Task<List<QuartzTask>> GetTasks()
        {
            var json = await File.ReadAllTextAsync(Path.Combine(_environment.ContentRootPath, "Config", "task.json"));
            _tasks = json.ToObject<List<QuartzTask>>() ?? new List<QuartzTask>();
            return _tasks;
        }

        /// <summary>
        /// 设置任务状态
        /// </summary>
        /// <returns></returns>
        public async Task SetTaskInfo(QuartzTask task)
        {
            var model = _tasks.Find(c => c.TaskID == task.TaskID);
            if (model == null) return;
            model.RunStatus = task.RunStatus;
            model.LastRunTime = task.LastRunTime;
            model.LastKey = task.LastKey;
            model.ElapsedTime = task.ElapsedTime;
            await Task.CompletedTask;
        }
    }
}