using Gksyb.Common.EventBus;
using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Core.Interfaces.WorkFlow;
using Gksyb.Model.WorkFlow;
using Gksyb.Workflow.EventSubscriber.Dtos;
using Gksyb.Workflow.Services.Workflow.Dtos;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

namespace Gksyb.Workflow.Services.Workflow.Bpmn
{
    /// <summary>
    /// 节点服务
    /// </summary>
    public abstract class BpmnNodeService : BpmnBaseService
    {
        protected readonly IServiceProvider _serviceProvider;
        protected bool _isTask = false;

        private ScopeUser _user;

        protected ScopeUser User
        {
            get
            {
                if (_user != null) return _user;
                _user = _serviceProvider.GetService<ScopeUser>();
                return _user;
            }
        }

        public BpmnNodeService(IDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext)
        {
            _serviceProvider = serviceProvider;
        }

        public new void Init(FlowGraphNode flowGraphNode)
        {
            base.Init(flowGraphNode);
        }

        /// <summary>
        /// 输入
        /// </summary>
        public List<BpmnSequenceFlowService> Inputs { get; set; } = new();

        /// <summary>
        /// 输出
        /// </summary>
        public List<BpmnSequenceFlowService> Outputs { get; set; } = new();

        /// <summary>
        /// 执行模型
        /// </summary>
        protected abstract Task Exec(FlowExecuteInfo info);

        public override async Task Execute(FlowExecuteInfo info)
        {
            await Intercept(PreInterceptors, info);
            await Exec(info);
            await Intercept(PostInterceptors, info);
        }

        /// <summary>
        /// 完成当前节点
        /// </summary>
        public override async Task Complate(FlowExecuteInfo info)
        {
            info.NodeStatus ??= WF_NODEExtensions.Agree;
            await ComplateTask(info);
            switch (info.NodeStatus)
            {
                case WF_NODEExtensions.Agree:
                    if (!info.ToNodeIsEmpty) break;
                    await ExecuteOutputs(info);
                    break;

                case WF_NODEExtensions.Back:
                    //退回同时完成同节点的其他任务
                    await ComplateTask(c => c.TASK_ID == info.TaskId && c.NODE_ID == Id && c.NODE_STATUS == WF_NODEExtensions.Active, c =>
                    {
                        c.NODE_STATUS = WF_NODEExtensions.Archived;
                    });
                    break;

                case WF_NODEExtensions.Transfer:
                    await AddTask(info);
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 加入日志
        /// </summary>
        public async Task AddLog(FlowExecuteInfo info, string operType)
        {
            var id = GuidHelper.NewSnowflakeId();
            await _dbContext.InsertAsync(() => new WF_TASK_LOG()
            {
                ID = id,
                TASK_ID = info.TaskId,
                WF_NODE_ID = info.Id,
                NODE_ID = Id,
                OPERATOR = User.RealName,
                OPERTITLE = Title,
                OPERTYPE = operType,
                OPERDETAIL = info.NodeReason,
                OPERDATE = DateTime.Now
            });
        }

        /// <summary>
        /// 节点转任务
        /// </summary>
        protected async Task<string> AddTask(FlowExecuteInfo info, bool publishEvent = true)
        {
            var users = info.Users;
            info.Users = null;
            if (users == null || users.Count < 1)
            {
                var operatorType = OperatorType ?? "";
                if (string.IsNullOrWhiteSpace(operatorType)) return info.NodeId;
                var service = _serviceProvider.GetService<IUserService>();
                var corpid = await _dbContext.Query<WF_TASK>().Where(c => c.ID == info.TaskId).Select(c => c.CORPID).FirstOrDefaultAsync();
                users = await service.FindOperators(new FindOperatorInfo()
                {
                    Type = operatorType,
                    Corp = corpid,
                    Operators = Operators
                });
            }
            if (users.Count < 1) throw new MessageException($"找不到下一节点的处理人");
            var nodeId = info.NodeId;
            var sysdate = await _dbContext.GetSysdate();
            var nodes = new List<WF_NODE>();
            foreach (var user in users)
            {
                var node = new WF_NODE()
                {
                    ID = GuidHelper.NewShortId(),
                    FLOW_ID = info.FlowId,
                    TASK_ID = info.TaskId,
                    NODE_ID = Id,
                    NODE_NAME = Name,
                    NODE_TITLE = Title,
                    NODE_TYPE = BpmnType,
                    NODE_USERID = user.Id,
                    NODE_USERNAME = user.Account,
                    NODE_USER = user.Name,
                    NODE_STATUS = WF_NODEExtensions.Active,
                    TO_NODE_ID = info.ToNode,
                    CREATEUSER = User.RealName,
                    CREATEDATE = sysdate
                };
                await _dbContext.InsertAsync(node);
                nodes.Add(node);
                nodeId = node.ID;
            }
            if (publishEvent) await EventPublish(WorkflowEventAction.AddTask, nodes);
            info.ToNode = null;
            return nodeId;
        }

        /// <summary>
        /// 完成任务并发布事件
        /// </summary>
        protected async Task<List<WF_NODE>> ComplateTask(Expression<Func<WF_NODE, bool>> expression, Action<WF_NODE> action, bool isAll = false)
        {
            var query = _dbContext.Query<WF_NODE>().Where(expression);
            query = isAll ? query : query.Select(c => new WF_NODE()
            {
                ID = c.ID,
                FLOW_ID = c.FLOW_ID,
                TASK_ID = c.TASK_ID,
                NODE_TITLE = c.NODE_TITLE,
                NODE_STATUS = c.NODE_STATUS,
                NODE_REASON = c.NODE_REASON,
                FINISHDATE = c.FINISHDATE
            });
            var nodes = await query.ToListAsync();
            if (nodes.Count < 1) return nodes;
            var sysdate = await _dbContext.GetSysdate();
            foreach (var node in nodes)
            {
                if (node.FINISHDATE.HasValue) continue;
                _dbContext.TrackEntity(node);
                node.FINISHDATE = sysdate;
                action(node);
                await _dbContext.UpdateAsync(node);
            }
            return nodes;
        }

        /// <summary>
        /// 节点转任务
        /// </summary>
        protected async Task ComplateTask(FlowExecuteInfo info)
        {
            await ComplateTask(c => c.ID == info.Id && c.NODE_STATUS == WF_NODEExtensions.Active, c =>
            {
                c.NODE_STATUS = info.NodeStatus;
                c.NODE_REASON = info.NodeReason;
            });
        }

        /// <summary>
        /// 完成来源节点的任务
        /// </summary>
        protected async Task ComplatePreviousTask(FlowExecuteInfo info, List<BpmnSequenceFlowService> inputs)
        {
            if (inputs == null || inputs.Count < 1) return;
            foreach (var input in inputs)
            {
                if (input.Source == null) continue;
                if (input.Source._isTask)
                {
                    await ComplateTask(c => c.TASK_ID == info.TaskId && c.NODE_ID == input.Source.Id && c.NODE_STATUS == WF_NODEExtensions.Active, c =>
                    {
                        c.NODE_STATUS = WF_NODEExtensions.Archived;
                    });
                }
                await ComplatePreviousTask(info, input.Source.Inputs);
            }
        }

        /// <summary>
        /// 获取来源节点的任务ID
        /// </summary>
        protected List<string> GetPreviousNodeNames(List<BpmnSequenceFlowService> inputs)
        {
            var nodes = new List<string>();
            GetPreviousNodeNames(inputs, nodes);
            return nodes;
        }

        /// <summary>
        /// 获取来源节点的任务ID
        /// </summary>
        private void GetPreviousNodeNames(List<BpmnSequenceFlowService> inputs, List<string> nodes)
        {
            if (inputs == null || inputs.Count < 1) return;
            foreach (var input in inputs)
            {
                if (input.Source == null) continue;
                if (input.Source._isTask)
                {
                    nodes.Add(input.Source.Name);
                    continue;
                }
                GetPreviousNodeNames(input.Source.Inputs, nodes);
            }
        }

        /// <summary>
        /// 事件发布
        /// </summary>
        public async Task EventPublish(string action, List<WF_NODE> nodes)
        {
            if (nodes.Count < 1) return;
            var taskId = nodes.First().TASK_ID;
            var flow = await _dbContext.Query<WF_TASK>().Where(c => c.ID == taskId).Select(c => new WF_TASK()
            {
                FLOW_NAME = c.FLOW_NAME,
                FLOW_TITLE = c.FLOW_TITLE,
                APPNAME = c.APPNAME
            }).FirstOrDefaultAsync();
            var eventPublisher = _serviceProvider.GetService<IEventPublisher>();
            var message = new MessageInfo()
            {
                Title = flow.FLOW_NAME,
                Content = flow.FLOW_TITLE,
                Appname = flow.APPNAME
            };
            foreach (var node in nodes)
            {
                message.Key = node.ID;
                message.Receives = new List<string>() { node.NODE_USERNAME };
                message.Data = WF_NODEExtensions.GetDesc(node.NODE_STATUS);
                await eventPublisher.PublishAsync(new ActionData<MessageInfo>()
                {
                    Action = action,
                    Data = message
                });
            }
        }

        /// <summary>
        /// 执行后续节点
        /// </summary>
        protected async Task ExecuteOutputs(FlowExecuteInfo info) => await Outputs.ForEachAsync(async c => await c.Execute(info));

        private List<IFlowInterceptor> preInterceptors;

        /// <summary>
        /// 前置拦截器
        /// </summary>
        public List<IFlowInterceptor> PreInterceptors
        {
            get
            {
                if (preInterceptors != null) return preInterceptors;
                preInterceptors = GetFlowInterceptor(PreInterceptor);
                return preInterceptors;
            }
        }

        private List<IFlowInterceptor> postInterceptors;

        /// <summary>
        /// 前置拦截器
        /// </summary>
        public List<IFlowInterceptor> PostInterceptors
        {
            get
            {
                if (postInterceptors != null) return postInterceptors;
                postInterceptors = GetFlowInterceptor(PostInterceptor);
                return postInterceptors;
            }
        }

        /// <summary>
        /// 根据顺序，获取拦截器
        /// </summary>
        private List<IFlowInterceptor> GetFlowInterceptor(string interceptor)
        {
            var interceptors = (interceptor ?? "").Split(',').Where(c => !string.IsNullOrWhiteSpace(c));
            if (!interceptors.Any()) return new List<IFlowInterceptor>();
            var list = new List<IFlowInterceptor>();
            interceptors.ForEach(c =>
            {
                if (_serviceProvider.GetService(a => a.ServiceType.Name == c && a.ServiceType is IFlowInterceptor) is IFlowInterceptor service)
                {
                    list.Add(service);
                }
            });
            return list;
        }

        /// <summary>
        /// 拦截方法
        /// </summary>
        private static async Task Intercept(List<IFlowInterceptor> interceptorList, FlowExecuteInfo taskInfo)
        {
            await interceptorList.ForEachAsync(async c =>
            {
                await c.Intercept(taskInfo);
            });
        }

        /// <summary>
        /// 前置拦截器
        /// </summary>
        private string PreInterceptor
        {
            get
            {
                return GetProperties("preInterceptors") as string;
            }
        }

        /// <summary>
        /// 后置拦截器
        /// </summary>
        private string PostInterceptor
        {
            get
            {
                return GetProperties("postInterceptors") as string;
            }
        }

        /// <summary>
        /// 操作人类型
        /// </summary>
        private string OperatorType
        {
            get
            {
                return GetProperties("operatorType").CastTo<string>();
            }
        }

        /// <summary>
        /// 操作人
        /// </summary>
        private string Operators
        {
            get
            {
                return GetProperties("operators").CastTo<string>();
            }
        }
    }
}