using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.WorkFlow;
using Gksyb.Model.WorkFlow;
using Gksyb.Workflow.EventSubscriber.Dtos;
using Gksyb.Workflow.Services.Workflow.Bpmn;
using Gksyb.Workflow.Services.Workflow.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace Gksyb.Workflow.Services.Workflow
{
    public class TaskService : IBaseService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDbContext _dbContext;
        private readonly IUserService _userService;
        private readonly ScopeUser _user;

        public TaskService(IServiceProvider serviceProvider, IDbContext dbContext, ScopeUser user, IUserService userService)
        {
            _serviceProvider = serviceProvider;
            _dbContext = dbContext;
            _userService = userService;
            _user = user;
        }

        /// <summary>
        /// 启动流程
        /// </summary>
        public async Task StartAsync(FlowExecuteInfo info)
        {
            if (string.IsNullOrWhiteSpace(info.Id))
            {
                await Init(info);
            }
            else
            {
                await FindNodeService(info);
            }
            await _dbContext.UseTransactionAsync(async () =>
            {
                await StartNode.Execute(info);
                if (info.ToNodeIsEmpty) return;
                var toNodeService = Nodes.FirstOrDefault(c => c.Id == info.ToNode);
                info.ToNode = null;
                if (toNodeService != null) await toNodeService.Execute(info);
            });
        }

        /// <summary>
        /// 执行任务
        /// </summary>
        public async Task ExcuteAsync(FlowExecuteInfo info)
        {
            var nodeService = await FindNodeService(info);
            await _dbContext.UseTransactionAsync(async () =>
            {
                await nodeService.AddLog(info, NodeStatus.GetDesc(info.NodeStatus));
                await nodeService.Complate(info);
                if (!info.ToNodeIsEmpty)
                {
                    var toNodeService = Nodes.FirstOrDefault(c => c.Id == info.ToNode);
                    info.ToNode = info.NodeStatus == NodeStatus.Back ? nodeService.Id : null;
                    if (toNodeService != null) await toNodeService.Execute(info);
                }
                if (info.NodeStatus == NodeStatus.Agree)
                {
                    await AutoAgreeAsync(info);
                }
            });
        }

        /// <summary>
        /// 相同处理人自动同意
        /// </summary>
        private async Task AutoAgreeAsync(FlowExecuteInfo executeInfo)
        {
            var nodeInfos = executeInfo.ToNodeInfos.FindAll(c => c.NodeUserId == executeInfo.NodeUserId);
            if (nodeInfos.Count < 1) return;
            var info = executeInfo.MapTo<FlowExecuteInfo>();
            info.NodeReason = "自动流转";
            info.ToNodeInfos.Clear();
            foreach (var nodeInfo in nodeInfos)
            {
                info.Id = nodeInfo.Id;
                info.NodeId = nodeInfo.NodeId;
                info.Users?.Clear();
                info.ToNode = null;
                var nodeService = Nodes.FirstOrDefault(c => c.Id == nodeInfo.NodeId);
                await nodeService.AddLog(info, NodeStatus.GetDesc(info.NodeStatus));
                await nodeService.Complate(info);
                if (info.ToNodeIsEmpty)
                {
                    var toNodeService = Nodes.FirstOrDefault(c => c.Id == info.ToNode);
                    info.ToNode = info.NodeStatus == NodeStatus.Back ? nodeService.Id : null;
                    if (toNodeService != null) await toNodeService.Execute(info);
                }
            }
            await AutoAgreeAsync(info);
        }

        /// <summary>
        /// 取消流程
        /// </summary>
        public async Task CancelAsync(FlowExecuteInfo info)
        {
            var task = await _dbContext.Query<WF_TASK>().Where(c => c.ID == info.TaskId).Exclude(c => new { c.FLOW_FORM_DATA }).FirstOrDefaultAsync();
            MessageException.ThrowIf(task == null, "任务已结束");
            MessageException.ThrowIf(!_user.IsSuper && task.CREATEUSERID != _user.UserID, "您无权进行此操作");
            info.FlowId = task.FLOW_ID;
            info.NodeStatus = NodeStatus.Cancel;
            await Init(info);
            await _dbContext.UseTransactionAsync(async () =>
            {
                await EndNode.AddLog(info, NodeStatus.GetDesc(info.NodeStatus));
                await EndNode.Execute(info);
            });
        }

        /// <summary>
        /// 标记成已阅
        /// </summary>
        public async Task ReadAsync(List<string> ids)
        {
            if (ids == null || ids.Count < 1) return;
            await _dbContext.UpdateAsync<WF_NODE_SHARE>(c => ids.Contains(c.TASK_ID) && (c.USERID == _user.UserID || _user.IsSuper), c => new WF_NODE_SHARE()
            {
                FINISHDATE = DateTime.Now
            });
        }

        /// <summary>
        /// 全部标记成已阅
        /// </summary>
        public async Task ReadAllAsync()
        {
            await _dbContext.UpdateAsync<WF_NODE_SHARE>(c => c.USERID == _user.UserID && c.FINISHDATE == null, c => new WF_NODE_SHARE()
            {
                FINISHDATE = DateTime.Now
            });
        }

        /// <summary>
        /// 抄送任务
        /// </summary>
        public async Task ShareAsync(FlowExecuteInfo info)
        {
            WF_NODE node = null;
            var nodeService = await FindNodeService(info, c =>
            {
                node = c;
            });
            if (info.Users.Count < 1) return;
            var finishFlag = await _dbContext.Query<WF_TASK>().Where(c => c.ID == node.TASK_ID).Select(c => c.FINISHDATE.HasValue ? "1" : "0").FirstOrDefaultAsync();
            var nodes = new List<WF_NODE>();
            await _dbContext.UseTransactionAsync(async () =>
            {
                var sysdate = await _dbContext.GetSysdate();
                foreach (var user in info.Users)
                {
                    var isExists = await _dbContext.Query<WF_NODE_SHARE>().Where(c => c.USERID == user.Id && c.TASK_ID == node.TASK_ID && c.FINISHDATE == null).AnyAsync();
                    if (isExists) continue;
                    var shareNode = node.MapTo<WF_NODE>();
                    shareNode.ID = GuidHelper.NewShortId();
                    shareNode.NODE_USERID = user.Id;
                    shareNode.NODE_USER = user.Name;
                    shareNode.NODE_STATUS = NodeStatus.Share;
                    shareNode.CREATEUSER = _user.RealName;
                    shareNode.CREATEDATE = sysdate;
                    shareNode.VIEWDATE = sysdate;
                    shareNode.FINISHDATE = sysdate;
                    await _dbContext.InsertAsync(shareNode);
                    var share = new WF_NODE_SHARE
                    {
                        ID = GuidHelper.NewSnowflakeId(),
                        TASK_ID = shareNode.TASK_ID,
                        NODE_ID = shareNode.ID,
                        USERID = user.Id,
                        USER = user.Name,
                        CREATEUSER = _user.RealName,
                        CREATEDATE = DateTime.Now,
                        TASK_FINISH_FLAG = finishFlag
                    };
                    await _dbContext.InsertAsync(share);
                    nodes.Add(shareNode);
                }
                var reason = string.IsNullOrWhiteSpace(info.NodeReason) ? "" : $"{info.NodeReason}：";
                info.NodeReason = $"{reason}{info.Users.Select(c => c.Name).ToStr(",")}";
                await nodeService.AddLog(info, "抄送");
            });
            await nodeService.EventPublish(WorkflowEventAction.AddShare, nodes);
        }

        /// <summary>
        /// 转办
        /// </summary>
        public async Task TransferAsync(FlowExecuteInfo info)
        {
            WF_NODE node = null;
            info.NodeStatus = NodeStatus.Transfer;
            var nodeService = await FindNodeService(info, c =>
            {
                node = c;
            });
            if (info.Users.Count < 1) return;
            MessageException.ThrowIf(node.NODE_USERID == info.Users[0].Id, "不能转给自己");
            var toNodeId = info.NodeId;
            await _dbContext.UseTransactionAsync(async () =>
            {
                var reason = string.IsNullOrWhiteSpace(info.NodeReason) ? "" : $"{info.NodeReason}：";
                info.NodeReason = $"{reason}{info.Users.Select(c => c.Name).DistinctAndOrderBy().ToStr(",")}";
                await nodeService.AddLog(info, "转办");
                await nodeService.Complate(info);
            });
        }

        /// <summary>
        /// 驳回、任意跳转任务
        /// </summary>
        public async Task ExcuteAndJump(FlowExecuteInfo info)
        {
            info.NodeStatus ??= NodeStatus.Back;
            await ExcuteAsync(info);
        }

        /// <summary>
        /// 查找任务节点
        /// </summary>
        private async Task<BpmnNodeService> FindNodeService(FlowExecuteInfo info, Action<WF_NODE> action = null)
        {
            var node = await _dbContext.Query<WF_NODE>().Where(c => c.ID == info.Id).FirstOrDefaultAsync()
               ?? throw new MessageException($"找不到{info.Id}的任务节点");
            MessageException.ThrowIf(!_user.IsSuper && node.NODE_USERID != _user.UserID, "您无权进行此操作");
            MessageException.ThrowIf(node.NODE_STATUS != NodeStatus.Active, "节点已完成");
            info.FlowId = node.FLOW_ID;
            info.TaskId = node.TASK_ID;
            info.NodeId = node.NODE_ID;
            info.NodeUserId = node.NODE_USERID;
            await Init(info);
            if (string.IsNullOrWhiteSpace(info.TaskKey) && info.FormData != null)
            {
                info.TaskKey = info.GetTaskKey();
            }
            var nodeService = Nodes.FirstOrDefault(c => c.Id == node.NODE_ID);
            info.ToNode = info.NodeStatus switch
            {
                NodeStatus.Back => await nodeService.GetBackNode(info) ?? StartNode.Id,
                NodeStatus.Reject => EndNode.Id,
                _ => info.ToNode,
            };
            if (info.ToNodeIsEmpty) info.ToNode = node.TO_NODE_ID;
            if (!info.ToNodeIsEmpty)
            {
                var toNode = Nodes.FirstOrDefault(c => c.Id == info.ToNode) ?? Nodes.FirstOrDefault(c => c.Name == info.ToNode);
                info.ToNode = toNode?.Id;
            }
            action?.Invoke(node);
            return nodeService;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private async Task Init(FlowExecuteInfo info)
        {
            if (isInit) return;
            var flow = await _dbContext.Query<WF_FLOW>()
                .WhereIfNotNullOrEmpty(info.FlowId, c => c.ID == info.FlowId)
                .WhereIfNotNullOrEmpty(info.FlowCode, c => c.FLOW_CODE == info.FlowCode && c.FLAG == "1").FirstOrDefaultAsync();
            MessageException.ThrowIf(flow == null, $"找不到编号{info.FlowId ?? info.FlowCode}的流程");
            var graphData = flow.FLOW_CONTENT.ToObject<FlowGraphData>();
            Nodes.Clear();
            Sequences.Clear();
            graphData.Nodes?.ForEach(c =>
            {
                var serviceName = $"Gksyb.Workflow.Services.Workflow.Bpmn.{c.ServiceName}";
                if (_serviceProvider.GetService(serviceName) is not BpmnNodeService service) return;
                service.Init(c);
                Nodes.Add(service);
            });
            graphData.Edges?.ForEach(c =>
            {
                var service = _serviceProvider.GetService<BpmnSequenceFlowService>();
                if (service is null) return;
                service.Init(c, Nodes);
                Sequences.Add(service);
            });
            info.Users = await FindUsers(info.Operators);
            info.CorpId = string.IsNullOrWhiteSpace(info.CorpId) ? _user.Corp?.CorpID : info.CorpId;
            if (string.IsNullOrWhiteSpace(info.CorpId))
            {
                info.CorpId = flow.CORPID;
            }
            info.AppName = flow.APPNAME;
            info.FlowId = flow.ID;
            info.FlowCode = flow.FLOW_CODE;
            info.FlowName = flow.FLOW_NAME;
            info.Title = flow.FLOW_TITLE;
            info.KeyName = flow.KEY_NAME;
            isInit = true;
        }

        /// <summary>
        /// 根据ID获取用户信息
        /// </summary>
        private async Task<List<UserInfo>> FindUsers(string opers)
        {
            var users = (opers ?? "").Split("@#").DistinctAndOrderBy().Select(c => c.CastTo<long?>()).ToList();
            if (users.Count < 1) return new List<UserInfo>();
            return await _userService.Find(users, true);
        }

        private bool isInit = false;

        private BpmnStartEventService StartNode => Nodes.FirstOrDefault(c => c is BpmnStartEventService) as BpmnStartEventService;

        private BpmnEndEventService EndNode => Nodes.FirstOrDefault(c => c is BpmnEndEventService) as BpmnEndEventService;

        /// <summary>
        /// 任务节点
        /// </summary>
        private List<BpmnNodeService> Nodes { get; set; } = new List<BpmnNodeService>();

        /// <summary>
        /// 变迁
        /// </summary>
        private List<BpmnSequenceFlowService> Sequences { get; set; } = new List<BpmnSequenceFlowService> { };
    }
}