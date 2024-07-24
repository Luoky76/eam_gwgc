using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.WorkFlow;
using Gksyb.Model.WorkFlow;
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
        private bool _doPostInterceptors = false;

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
        protected abstract Task Exec();

        public override async Task Execute()
        {
            await Intercept(PreInterceptors);
            await Exec();
            await DoPostInterceptors();
        }

        /// <summary>
        /// 完成当前节点
        /// </summary>
        public override async Task Complate()
        {
            if (_info.NodeStatus == NodeStatus.Back) //退回特殊处理
            {
                await ComplateTask(c => c.TASK_ID == _info.TaskId && c.NODE_STATUS == NodeStatus.Active, c =>
                {
                    c.NODE_STATUS = NodeStatus.BackArchived;
                });
                return;
            }
            _info.NodeStatus ??= NodeStatus.Agree;
            await ComplateTask();
            switch (_info.NodeStatus)
            {
                case NodeStatus.Agree:
                    if (!_info.ToNodeIsEmpty) break;
                    await ExecuteOutputs();
                    break;

                case NodeStatus.Transfer:
                    await AddTask();
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// 获取退回节点
        /// </summary>
        public async Task<string> GetBackNode()
        {
            var backNode = BackNode;
            if (string.IsNullOrWhiteSpace(backNode) || backNode == "start") return null;
            if (backNode != "previous") return backNode;
            var nodes = new List<string>();
            PreviousTaskForEach(Inputs, c =>
            {
                nodes.Add(c.Id);
                return false;
            });
            var node = await _dbContext.Query<WF_NODE>().Where(c => c.TASK_ID == _info.TaskId && nodes.Contains(c.NODE_ID) && c.NODE_STATUS == NodeStatus.Agree)
                .Select(c => new WF_NODE()
                {
                    ID = c.ID,
                    NODE_ID = c.NODE_ID,
                    FINISHDATE = c.FINISHDATE
                })
                .OrderByDesc(c => c.FINISHDATE).FirstOrDefaultAsync();
            return node?.NODE_ID;
        }

        /// <summary>
        /// 加入日志
        /// </summary>
        public async Task AddLog(string operType)
        {
            var id = GuidHelper.NewSnowflakeId();
            await _dbContext.InsertAsync(() => new WF_TASK_LOG()
            {
                ID = id,
                TASK_ID = _info.TaskId,
                WF_NODE_ID = _info.Id,
                NODE_ID = Id,
                OPERATOR = User.RealName,
                OPERTITLE = Title,
                OPERTYPE = operType,
                OPERDETAIL = _info.NodeReason,
                OPERDATE = DateTime.Now
            });
        }

        /// <summary>
        /// 节点转任务
        /// </summary>
        protected async Task<string> AddTask()
        {
            var users = _info.Users ?? new List<UserInfo>();
            _info.Users = null;
            if (users.Count < 1 && !string.IsNullOrWhiteSpace(OperatorType))
            {
                users = OperatorType == "FromService" ?
                    await FindeOperatorsFromService() :
                    await FindeOperators();
            }
            if (users.Count < 1)
            {
                if (AutoNext)
                {
                    await ExecuteOutputs();
                    await DoPostInterceptors();
                    return _info.NodeId;
                }
                throw new MessageException($"找不到下一节点的处理人");
            }
            var nodeId = _info.NodeId;
            var sysdate = await _dbContext.GetSysdate();
            var nodes = new List<WF_NODE>();
            foreach (var user in users)
            {
                var node = new WF_NODE()
                {
                    ID = GuidHelper.NewShortId(),
                    FLOW_ID = _info.FlowId,
                    TASK_ID = _info.TaskId,
                    NODE_ID = Id,
                    NODE_NAME = Name,
                    NODE_TITLE = Title,
                    NODE_TYPE = BpmnType,
                    NODE_USERID = user.Id,
                    NODE_USERNAME = user.Account,
                    NODE_USER = user.Name,
                    NODE_STATUS = NodeStatus.Active,
                    TO_NODE_ID = _info.ToNode,
                    CREATEUSER = User.RealName,
                    CREATEDATE = sysdate
                };
                await _dbContext.InsertAsync(node);
                nodes.Add(node);
                nodeId = node.ID;
            }
            var nodeInfos = nodes.Select(c => c.ToNodeInfo()).ToList();
            _info.ToNode = null;
            _info.ToNodeInfos.AddRange(nodeInfos);
            _info.ToDos.AddRange(nodeInfos);
            return nodeId;
        }

        /// <summary>
        /// 完成任务并发布事件
        /// </summary>
        protected async Task<List<WF_NODE>> ComplateTask(Expression<Func<WF_NODE, bool>> expression, Action<WF_NODE> action)
        {
            var allNodes = await _dbContext.Query<WF_NODE>().Where(expression).ToListAsync();
            var nodes = allNodes.FindAll(c => !c.FINISHDATE.HasValue);
            if (nodes.Count < 1) return allNodes;
            var sysdate = await _dbContext.GetSysdate();
            foreach (var node in nodes)
            {
                _dbContext.TrackEntity(node);
                node.FINISHDATE = sysdate;
                action(node);
                await _dbContext.UpdateAsync(node);
            }
            var nodeInfos = nodes.Select(c => c.ToNodeInfo()).ToList();
            _info.Dones.AddRange(nodeInfos);
            return allNodes;
        }

        /// <summary>
        /// 完成任务并发布事件
        /// </summary>
        protected async Task ComplateTask()
        {
            await ComplateTask(c => c.ID == _info.Id && c.NODE_STATUS == NodeStatus.Active, c =>
            {
                c.NODE_STATUS = _info.NodeStatus;
                c.NODE_REASON = _info.NodeReason;
            });
        }

        /// <summary>
        /// 完成来源节点的任务
        /// </summary>
        protected async Task ComplatePreviousTask(List<BpmnSequenceFlowService> inputs)
        {
            if (inputs == null || inputs.Count < 1) return;
            foreach (var input in inputs)
            {
                if (input.Source == null) continue;
                if (input.Source._isTask)
                {
                    await ComplateTask(c => c.TASK_ID == _info.TaskId && c.NODE_ID == input.Source.Id && c.NODE_STATUS == NodeStatus.Active, c =>
                    {
                        c.NODE_STATUS = NodeStatus.Archived;
                    });
                }
                await ComplatePreviousTask(input.Source.Inputs);
            }
        }

        /// <summary>
        /// 获取来源任务节点的名称，只取所有变迁的上一个任务节点
        /// </summary>
        protected static List<string> GetPreviousNodeNames(List<BpmnSequenceFlowService> inputs)
        {
            var nodes = new List<string>();
            PreviousTaskForEach(inputs, c =>
            {
                nodes.Add(c.Name);
                return false;//通过返回false，追到任务节点，这个变迁就停止追踪
            });
            return nodes;
        }

        /// <summary>
        /// 来源任务节点遍历
        /// </summary>
        private static void PreviousTaskForEach(List<BpmnSequenceFlowService> inputs, Func<BpmnBaseService, bool> func)
        {
            if (inputs == null || inputs.Count < 1) return;
            foreach (var input in inputs)
            {
                if (input.Source == null) continue;
                if (input.Source._isTask)
                {
                    if (!func(input.Source)) continue;
                }
                PreviousTaskForEach(input.Source.Inputs, func);
            }
        }

        /// <summary>
        /// 加入发布事件
        /// </summary>
        public void AddEvent(string action, List<WF_NODE> nodes)
        {
            if (nodes.Count < 1) return;
            var eventData = FlowEventInfo.FromFlowExecuteInfo(_info);
            eventData.NodeInfos = nodes.Select(c => c.ToNodeInfo()).ToList();
            _info.Events.Add(new ActionData<FlowEventInfo>()
            {
                Action = action,
                Data = eventData
            });
        }

        /// <summary>
        /// 执行后续节点
        /// </summary>
        protected async Task ExecuteOutputs() => await Outputs.ForEachAsync(async c => await c.Execute());

        private List<IFlowInterceptor> preInterceptors;

        /// <summary>
        /// 前置拦截器
        /// </summary>
        private List<IFlowInterceptor> PreInterceptors
        {
            get
            {
                if (preInterceptors != null) return preInterceptors;
                preInterceptors = GetFlowInterceptor(PreInterceptor);
                return preInterceptors;
            }
        }

        /// <summary>
        /// 执行后置拦截器
        /// </summary>
        protected async Task DoPostInterceptors()
        {
            if (_doPostInterceptors) return;
            _doPostInterceptors = true;
            await Intercept(PostInterceptors);
        }

        private List<IFlowInterceptor> postInterceptors;

        /// <summary>
        /// 前置拦截器
        /// </summary>
        private List<IFlowInterceptor> PostInterceptors
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
                if (_serviceProvider.GetService(a => (a.ImplementationType ?? a.ServiceType).FullName == c) is IFlowInterceptor service)
                {
                    list.Add(service);
                }
            });
            return list;
        }

        /// <summary>
        /// 拦截方法
        /// </summary>
        private async Task Intercept(List<IFlowInterceptor> interceptorList)
        {
            if (interceptorList.Count < 1) return;
            await SetFormData();
            await interceptorList.ForEachAsync(async c =>
            {
                await c.Intercept(_info);
            });
        }

        /// <summary>
        /// 从自定义的服务里面找处理人
        /// </summary>
        /// <returns></returns>
        private async Task<List<UserInfo>> FindeOperatorsFromService()
        {
            var serviceName = Operators;
            if (string.IsNullOrWhiteSpace(serviceName)) return new List<UserInfo>();
            var service = _serviceProvider.GetService(a => (a.ImplementationType ?? a.ServiceType).FullName == serviceName) as IFindOperators;
            await SetFormData();
            return await service.Find(_info);
        }

        /// <summary>
        /// 从预制的服务里面找处理人
        /// </summary>
        /// <returns></returns>
        private async Task<List<UserInfo>> FindeOperators()
        {
            var service = _serviceProvider.GetService<IUserService>();
            var corpid = await _dbContext.Query<WF_TASK>().Where(c => c.ID == _info.TaskId).Select(c => c.CORPID).FirstOrDefaultAsync();
            return await service.FindOperators(new FindOperatorInfo()
            {
                Type = OperatorType,
                Corp = corpid,
                Operators = Operators
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

        /// <summary>
        /// 自动流转
        /// </summary>
        private bool AutoNext
        {
            get
            {
                return GetProperties("autoNext").CastTo(false);
            }
        }

        /// <summary>
        /// 退回节点
        /// </summary>
        private string BackNode
        {
            get
            {
                return GetProperties("backNode").CastTo<string>();
            }
        }
    }
}