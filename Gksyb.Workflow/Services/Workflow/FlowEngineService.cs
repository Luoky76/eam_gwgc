using Gksyb.Core.Interfaces.WorkFlow;
using Gksyb.Model.WorkFlow;
using System.Linq.Expressions;

namespace Gksyb.Workflow.Services.Workflow
{
    public class FlowEngineService : IFlowEngineService
    {
        private readonly IDbContext _dbContext;
        private readonly TaskService _taskService;

        public FlowEngineService(IDbContext dbContext, TaskService taskService)
        {
            _dbContext = dbContext;
            _taskService = taskService;
        }

        /// <inheritdoc/>
        public async Task<List<FlowInfo>> FlowListAsync(Expression<Func<FlowInfo, bool>> filter = null)
        {
            return await _dbContext.Query<WF_FLOW>().Where(c => c.FLAG == "1")
                .Select(c => new FlowInfo()
                {
                    Id = c.ID,
                    FlowCode = c.FLOW_CODE,
                    FlowName = c.FLOW_NAME,
                    FlowGroup = c.FLOW_GROUP,
                    FlowTitle = c.FLOW_TITLE,
                    FlowOrder = c.FLOW_ORDER,
                    FlowFormUrl = c.FLOW_FORM_URL,
                    FlowFormMobileUrl = c.FLOW_FORM_MOBILE_URL,
                    FlowVersion = c.FLOW_VERSION,
                    Corpid = c.CORPID
                }).WhereIfNotNull(filter, filter).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<TaskInfo> TaskInfoAsync(Expression<Func<TaskInfo, bool>> filter = null)
        {
            var info = await TaskInfoInnerAsync<WF_TASK, WF_TASK_LOG>(filter);
            if (info != null) return info;
            return await TaskInfoInnerAsync<WF_HISTORY_TASK, WF_HISTORY_TASK_LOG>(filter);
        }

        /// <inheritdoc/>
        public async Task<List<TaskLog>> TaskLogAsync(string taskId)
        {
            var info = await TaskInfoAsync(c => c.TaskId == taskId);
            return info?.Logs;
        }

        /// <inheritdoc/>
        public Task StartAsync(FlowExecuteInfo info) => _taskService.StartAsync(info);

        /// <inheritdoc/>
        public Task ExcuteAsync(FlowExecuteInfo info) => _taskService.ExcuteAsync(info);

        /// <inheritdoc/>
        public Task TransferAsync(FlowExecuteInfo info) => _taskService.TransferAsync(info);

        /// <inheritdoc/>
        public Task CancelAsync(FlowExecuteInfo info) => _taskService.CancelAsync(info);

        /// <inheritdoc/>
        public Task ShareAsync(FlowExecuteInfo info) => _taskService.ShareAsync(info);

        /// <summary>
        /// 获取任务信息
        /// </summary>
        private async Task<TaskInfo> TaskInfoInnerAsync<T1, T2>(Expression<Func<TaskInfo, bool>> filter = null) where T1 : WF_TASK where T2 : WF_TASK_LOG
        {
            var info = await _dbContext.Query<T1>().InnerJoin<WF_FLOW>((task, flow) => task.FLOW_ID == flow.ID)
                .Select((task, flow) => new TaskInfo()
                {
                    TaskId = task.ID,
                    TaskKey = task.TASK_KEY,
                    FlowId = task.FLOW_ID,
                    FlowCode = flow.FLOW_CODE,
                    Title = task.FLOW_TITLE,
                    FormData = task.FLOW_FORM_DATA,
                    Creator = task.CREATEUSER,
                    CreateDate = task.CREATEDATE
                }).WhereIfNotNull(filter, filter).FirstOrDefaultAsync();
            if (info == null) return info;
            info.Logs = await TaskLogsInnerAsync<T2>(info.TaskId);
            return info;
        }

        /// <summary>
        /// 获取任务日志信息
        /// </summary>
        private async Task<List<TaskLog>> TaskLogsInnerAsync<T>(string taskId) where T : WF_TASK_LOG
        {
            return await _dbContext.Query<T>().Where(a => a.TASK_ID == taskId).Select(a => new TaskLog()
            {
                NodeId = a.NODE_ID,
                Operator = a.OPERATOR,
                OperType = a.OPERTYPE,
                OperTitle = a.OPERTITLE,
                OperDetail = a.OPERDETAIL,
                OperDate = a.OPERDATE
            }).ToListAsync();
        }
    }
}