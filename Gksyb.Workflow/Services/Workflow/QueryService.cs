using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.WorkFlow;
using Gksyb.Model.Grid;
using Gksyb.Model.WorkFlow;
using Gksyb.Workflow.Controllers.Workflow.Dtos;
using System.Linq.Expressions;

namespace Gksyb.Workflow.Services.Workflow
{
    public class QueryService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly UserSession _user;

        public QueryService(IDbContext dbContext, UserSession user)
        {
            _dbContext = dbContext;
            _user = user;
        }

        /// <summary>
        /// 流程列表
        /// </summary>
        public async Task<GridData> FlowListAsync(GridRequest request)
        {
            return await _dbContext.Query<WF_FLOW>()
                .Where(FilterCorp)
                .Where(_user.IsSuper ? c => true : c => (c.PASSIVE ?? "0") == "0")
                .Exclude(c => new { c.FLOW_CONTENT, c.FLOW_FORM }).GetGridData(request);
        }

        /// <summary>
        /// 获取待办
        /// </summary>
        public async Task<GridData> ToDoAsync(GridRequest request)
        {
            var query = _dbContext.Query<WF_NODE>().Where(node => node.NODE_STATUS == NodeStatus.Active)
                .InnerJoin<WF_TASK>((node, task) => node.TASK_ID == task.ID)
                .CorpFilter(_user).SelectNodeInfo();
            return await query.GetGridData(request);
        }

        /// <summary>
        /// 获取已办
        /// </summary>
        public async Task<GridData> DoneAsync(GridRequest request)
        {
            var query = _dbContext.Query<WF_NODE>().Where(node => node.FINISHDATE.HasValue)
               .InnerJoin<WF_TASK>((node, task) => node.TASK_ID == task.ID)
               .CorpFilter(_user).SelectNodeInfo();
            var data = await query.GetGridDataList(request);

            var history = _dbContext.Query<WF_HISTORY_NODE>()
                .InnerJoin<WF_HISTORY_TASK>((node, task) => node.TASK_ID == task.ID)
                .CorpFilter(_user).SelectNodeInfo();
            var historyData = await history.GetGridDataList(request);
            foreach (var row in historyData.Rows)
            {
                data.Rows.Add(row);
            }
            data.Total += historyData.Total;
            return new GridData()
            {
                Rows = data.Rows,
                Total = data.Total
            };
        }

        /// <summary>
        /// 我发起的
        /// </summary>
        public async Task<GridData> MyFlowAsync(GridRequest request)
        {
            var query = _dbContext.Query<WF_NODE>()
               .InnerJoin<WF_TASK>((node, task) => node.TASK_ID == task.ID && task.CREATEUSERID == _user.UserID)
               .SelectNodeInfo();
            var data = await query.GetGridDataList(request);

            var history = _dbContext.Query<WF_HISTORY_NODE>()
                .InnerJoin<WF_HISTORY_TASK>((node, task) => node.TASK_ID == task.ID && task.CREATEUSERID == _user.UserID)
                .SelectNodeInfo();
            var historyData = await history.GetGridDataList(request);
            var list = new List<NodeInfo>();
            list.AddRange(data.Rows as IList<NodeInfo>);
            list.AddRange(historyData.Rows as IList<NodeInfo>);
            list = list.OrderByDescending(c => c.FinishDate ?? DateTime.MaxValue).ThenBy(c => c.NodeStatus).DistinctBy(c => c.TaskId).ToList();
            return new GridData()
            {
                Rows = list,
                Total = list.Count
            };
        }

        /// <summary>
        /// 知会我的
        /// </summary>
        public async Task<GridData> ToReadAsync(GridRequest request)
        {
            var query = _dbContext.Query<WF_NODE_SHARE>().Where(c => c.TASK_FINISH_FLAG == "0")
               .Where(_user.IsSuper ? c => true : c => c.USERID == _user.UserID)
               .InnerJoin<WF_TASK>((share, task) => share.TASK_ID == task.ID).Select((share, task) => new NodeInfo
               {
                   Id = share.NODE_ID,
                   NodeStatus = share.FINISHDATE.HasValue ? 1 : 0,
                   StartDate = share.CREATEDATE,
                   ViewDate = share.VIEWDATE,
                   FinishDate = share.FINISHDATE,
                   Operator = share.USER,

                   TaskId = task.ID,
                   Creator = share.CREATEUSER,
                   CreateDate = share.CREATEDATE,
                   Title = task.FLOW_TITLE,
                   TaskFinishFlag = task.FINISHDATE.HasValue ? "1" : "0",
                   TaskFinishDate = task.FINISHDATE
               });
            var data = await query.GetGridDataList(request);
            var history = _dbContext.Query<WF_NODE_SHARE>().Where(c => c.TASK_FINISH_FLAG == "1")
               .Where(_user.IsSuper ? c => true : c => c.USERID == _user.UserID)
               .InnerJoin<WF_HISTORY_TASK>((share, task) => share.TASK_ID == task.ID).Select((share, task) => new NodeInfo
               {
                   Id = share.NODE_ID,
                   NodeStatus = share.FINISHDATE.HasValue ? 1 : 0,
                   StartDate = share.CREATEDATE,
                   ViewDate = share.VIEWDATE,
                   FinishDate = share.FINISHDATE,
                   Operator = share.USER,

                   TaskId = task.ID,
                   Creator = share.CREATEUSER,
                   CreateDate = share.CREATEDATE,
                   Title = task.FLOW_TITLE,
                   TaskFinishFlag = task.FINISHDATE.HasValue ? "1" : "0",
                   TaskFinishDate = task.FINISHDATE
               });
            var historyData = await history.GetGridDataList(request);
            var list = new List<NodeInfo>();
            list.AddRange(data.Rows as IList<NodeInfo>);
            list.AddRange(historyData.Rows as IList<NodeInfo>);
            return new GridData()
            {
                Rows = list,
                Total = list.Count
            };
        }

        /// <summary>
        /// 任务详情
        /// </summary>
        public async Task<TaskInfoEx> TaskInfoAsync(string id, string flowId)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return await GetTaskInfoInnerAsync(flowId);
            }
            var info = await GetTaskInfoInnerAsync<WF_NODE, WF_TASK, WF_TASK_LOG>(id) ?? await GetTaskInfoInnerAsync<WF_HISTORY_NODE, WF_HISTORY_TASK, WF_HISTORY_TASK_LOG>(id);
            MessageException.ThrowIf(info == null, $"找不到ID为{id}的节点");
            return info;
        }

        /// <summary>
        /// 初始任务详情
        /// </summary>
        private async Task<TaskInfoEx> GetTaskInfoInnerAsync(string flowId)
        {
            return await _dbContext.Query<WF_FLOW>().Where(FilterCorp).Where(c => c.ID == flowId)
                .Select(flow => new TaskInfoEx()
                {
                    FlowId = flow.ID,
                    Title = flow.FLOW_NAME,
                    FlowContent = flow.FLOW_CONTENT,
                    FormContent = flow.FLOW_FORM,
                    FormUrl = flow.FLOW_FORM_URL,
                    FormMobileUrl = flow.FLOW_FORM_MOBILE_URL
                }).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 从历史表中获取任务详情
        /// </summary>
        private async Task<TaskInfoEx> GetTaskInfoInnerAsync<T1, T2, T3>(string id) where T1 : WF_NODE, new() where T2 : WF_TASK where T3 : WF_TASK_LOG
        {
            var info = await _dbContext.Query<T1>().Where(node => node.ID == id)
                .InnerJoin<T2>((node, task) => node.TASK_ID == task.ID)
                .CorpFilter(_user)
                .InnerJoin<WF_FLOW>((node, task, flow) => task.FLOW_ID == flow.ID)
                .Select((node, task, flow) => new TaskInfoEx()
                {
                    Id = node.ID,
                    NodeId = node.NODE_ID,
                    NodeTitle = node.NODE_TITLE,
                    NodeType = node.NODE_TYPE,
                    NodeStatus = node.NODE_STATUS,
                    ViewDate = node.VIEWDATE,
                    TaskId = node.TASK_ID,
                    FlowId = node.FLOW_ID,
                    Title = task.FLOW_TITLE,
                    FlowContent = flow.FLOW_CONTENT,
                    FormContent = flow.FLOW_FORM,
                    FormUrl = flow.FLOW_FORM_URL,
                    FormMobileUrl = flow.FLOW_FORM_MOBILE_URL,
                    FormData = task.FLOW_FORM_DATA,
                    Creator = task.CREATEUSER,
                    CreateDate = task.CREATEDATE
                }).FirstOrDefaultAsync();
            if (info == null) return info;
            info.Logs = await _dbContext.Query<T3>().Where(a => a.TASK_ID == info.TaskId).Select(a => new TaskLog()
            {
                NodeId = a.NODE_ID,
                Operator = a.OPERATOR,
                OperType = a.OPERTYPE,
                OperTitle = a.OPERTITLE,
                OperDetail = a.OPERDETAIL,
                OperDate = a.OPERDATE
            }).ToListAsync();
            info.Logs = info.Logs.OrderByDescending(c => c.OperDate).ToList();
            if (!info.ViewDate.HasValue && _user.UserID == info.NodeUserId)
            {
                //更新查看时间
                await _dbContext.UpdateAsync<T1>(c => c.ID == info.Id, c => new T1()
                {
                    VIEWDATE = DateTime.Now
                });
            }
            if (info.NodeStatus == NodeStatus.Share)
            {
                await ReadAsync(info.Id);
            }
            return info;
        }

        /// <summary>
        /// 更新已阅
        /// </summary>
        private async Task ReadAsync(string id)
        {
            await _dbContext.UpdateAsync<WF_NODE_SHARE>(c => c.NODE_ID == id && c.FINISHDATE == null, c => new WF_NODE_SHARE()
            {
                FINISHDATE = DateTime.Now
            });
        }

        /// <summary>
        /// 过滤公司
        /// </summary>
        /// <returns></returns>
        private Expression<Func<WF_FLOW, bool>> FilterCorp => _user.IsAdmin ? c => c.FLAG == "1" :
            _user.ParentCompany == null ? c => c.FLAG == "1" && (c.CORPID == _user.Corp.CorpID || string.IsNullOrWhiteSpace(c.CORPID)) :
            c => c.FLAG == "1" && (c.CORPID == _user.Corp.CorpID || c.CORPID == _user.ParentCompany.CorpID || string.IsNullOrWhiteSpace(c.CORPID));
    }
}