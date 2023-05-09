using Gksyb.Common.Quartz.Dtos;

namespace Gksyb.Common.Quartz
{
    /// <summary>
    /// 任务存储
    /// </summary>
    public interface IQuartzStore
    {
        /// <summary>
        /// 获取任务
        /// </summary>
        /// <returns></returns>
        public Task<List<QuartzTask>> GetTasks();

        /// <summary>
        /// 设置任务状态
        /// </summary>
        /// <returns></returns>
        public Task SetTaskInfo(QuartzTask task);
    }
}