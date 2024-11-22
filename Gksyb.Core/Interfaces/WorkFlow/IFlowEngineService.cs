using System.Linq.Expressions;

namespace Gksyb.Core.Interfaces.WorkFlow
{
    /// <summary>
    /// 流程引擎服务
    /// </summary>
    public interface IFlowEngineService : IService
    {
        /// <summary>
        /// 获取流程列表
        /// </summary>
        public Task<List<FlowInfo>> FlowListAsync(Expression<Func<FlowInfo, bool>> filter = null);

        /// <summary>
        /// 获取任务详情
        /// </summary>
        public Task<TaskInfo> TaskInfoAsync(Expression<Func<TaskInfo, bool>> filter = null, bool hasNode = false);

        /// <summary>
        /// 获取任务流转意见
        /// </summary>
        public Task<List<TaskLog>> TaskLogAsync(string taskId);

        /// <summary>
        /// 启动流程 {"FlowCode":"流程编码","FormData":{"money":500}} 或 {"FlowId":"FlowId每次修改流程后会变（慎用）","FormData":{"money":500}}
        /// </summary>
        public Task StartAsync(FlowExecuteInfo info);

        /// <summary>
        /// 执行任务 {"Id":"2NSIfcMqWy4","NodeStatus":<see cref="NodeStatus"/>,"NodeReason":"同意"} {"TaskId":"2NSIfcMqWy4","NodeStatus":<see cref="NodeStatus"/>,"NodeReason":"同意"}
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

        /// <summary>
        /// 还原流程 {"TaskId":"2NSLakT4t9B"}
        /// </summary>
        public Task RestoreAsync(FlowExecuteInfo info);

        /// <summary>
        /// 设置任务的表单数据{"TaskId":"2NSLakT4t9B",FormData:{}}
        /// </summary>
        public Task SetFormDataAsync(FlowExecuteInfo info);
    }
}