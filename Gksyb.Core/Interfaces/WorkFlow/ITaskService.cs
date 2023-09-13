namespace Gksyb.Core.Interfaces.WorkFlow
{
    public interface ITaskService : IService
    {
        /// <summary>
        /// 启动流程 {"FlowId":"2I9BnRW0HmW","FormData":{"money":500}}
        /// </summary>
        public Task StartAsync(FlowExecuteInfo info);

        /// <summary>
        /// 执行任务 {"Id":"2NSIfcMqWy4","NodeStatus":<see cref="NodeStatus"/>,"NodeReason":"同意"}
        /// </summary>
        public Task ExcuteAsync(FlowExecuteInfo info);

        /// <summary>
        /// 抄送任务 {"Id":"2NSLalmc5KC","NodeReason":"","Operators":"12@#9"}
        /// </summary>
        public Task ShareAsync(FlowExecuteInfo info);

        /// <summary>
        /// 转办 {"Id":"2NSLalmc5KC","NodeReason":"","Operators":"12"}
        /// </summary>
        public Task TransferAsync(FlowExecuteInfo info);

        /// <summary>
        /// 取消流程 {"Id":"2NSLalmc5KB","TaskId":"2NSLakT4t9B"}
        /// </summary>
        public Task CancelAsync(FlowExecuteInfo info);
    }
}