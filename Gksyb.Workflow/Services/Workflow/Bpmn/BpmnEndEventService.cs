using Gksyb.Core.Interfaces.WorkFlow;
using Gksyb.Model.WorkFlow;

namespace Gksyb.Workflow.Services.Workflow.Bpmn
{
    [ServiceLifetime]
    public class BpmnEndEventService : BpmnNodeService, IBaseService
    {
        public BpmnEndEventService(IDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
        {
        }

        protected override async Task Exec()
        {
            var task = await _dbContext.Query<WF_TASK>().Where(c => c.ID == _info.TaskId).FirstOrDefaultAsync() ??
                throw new MessageException($"找不到流程任务");
            var logs = await _dbContext.Query<WF_TASK_LOG>().Where(c => c.TASK_ID == _info.TaskId).ToListAsync();

            //完成处理
            var sysdate = await _dbContext.GetSysdate();
            task.FLOW_STATUS = _info.NodeStatus == NodeStatus.Cancel ? WF_TASKExtensions.Cancel : WF_TASKExtensions.Finish;
            task.FINISHDATE = sysdate;
            var nodes = await ComplateTask(c => c.TASK_ID == _info.TaskId, c =>
            {
                c.NODE_STATUS = NodeStatus.Archived;
            });

            //更新抄送表的任务完成标志
            await _dbContext.UpdateAsync<WF_NODE_SHARE>(c => c.TASK_ID == _info.TaskId, c => new WF_NODE_SHARE()
            {
                TASK_FINISH_FLAG = "1"
            });

            //删除并插入历史表
            var node = nodes.OrderBy(c => c.ID).First();
            AddEvent(WorkflowEventAction.CompleteTask, new List<WF_NODE>() { new()
            {
                TASK_ID = _info.TaskId,
                ID = node.ID,
                NODE_USERNAME = node.NODE_USERNAME,
                NODE_STATUS = _info.NodeStatus
            }});
            await _dbContext.InsertAsync(task.MapTo<WF_HISTORY_TASK>());
            await _dbContext.InsertRangeAsync(nodes.MapTo<List<WF_HISTORY_NODE>>());
            await _dbContext.InsertRangeAsync(logs.MapTo<List<WF_HISTORY_TASK_LOG>>());
            await _dbContext.DeleteAsync(task);
            await nodes.ForEachAsync(async node =>
            {
                await _dbContext.DeleteAsync(node);
            });
            await logs.ForEachAsync(async log =>
            {
                await _dbContext.DeleteAsync(log);
            });
        }

        public override async Task Complate()
        {
            await Task.CompletedTask;
            throw new MessageException($"终止节点{Title}不能被完成");
        }
    }
}