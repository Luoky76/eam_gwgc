using Gksyb.Core.Filter;
using Gksyb.Core.Interfaces.WorkFlow;
using Gksyb.Model.WorkFlow;
using Gksyb.Workflow.Services.Workflow.Dtos;

namespace Gksyb.Workflow.Services.Workflow.Bpmn
{
    /// <summary>
    /// 流程基础模型
    /// </summary>
    public abstract class BpmnBaseService
    {
        protected readonly IDbContext _dbContext;
        protected FlowExecuteInfo _info;

        public BpmnBaseService(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void Init(FlowExecuteInfo info, FlowGraphNode flowGraphNode)
        {
            _info = info;
            Properties = flowGraphNode.Properties;
            Id = flowGraphNode.ID;
            Name = GetProperties("name") as string;
            Title = flowGraphNode.Title;
            BpmnType = flowGraphNode.Type;
        }

        /// <summary>
        /// 标识
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Bpmn类型
        /// </summary>
        public string BpmnType { get; set; }

        /// <summary>
        /// 属性
        /// </summary>
        public Dictionary<string, object> Properties { get; set; }

        /// <summary>
        /// 根据名称获取属性
        /// </summary>
        protected object GetProperties(string name)
        {
            if (Properties == null) return null;
            if (!Properties.ContainsKey(name)) return null;
            return Properties[name];
        }

        /// <summary>
        /// 获取当前表单数据
        /// </summary>
        protected async Task SetFormData()
        {
            if (_info.FormData != null) return;
            var task = await _dbContext.Query<WF_TASK>().Where(c => c.ID == _info.TaskId).Select(c => new WF_TASK()
            {
                ID = c.ID,
                TASK_KEY = c.TASK_KEY,
                FLOW_FORM_DATA = c.FLOW_FORM_DATA
            }).FirstOrDefaultAsync();
            task ??= await _dbContext.Query<WF_HISTORY_TASK>().Where(c => c.ID == _info.TaskId).Select(c => new WF_TASK()
            {
                ID = c.ID,
                TASK_KEY = c.TASK_KEY,
                FLOW_FORM_DATA = c.FLOW_FORM_DATA
            }).FirstOrDefaultAsync();
            _info.FormData = (task == null ? "" : (task.FLOW_FORM_DATA ?? "")).ToObject<Dictionary<string, object>>()
                ?? new Dictionary<string, object>();
            _info.TaskKey = task == null ? _info.GetTaskKey() : task.TASK_KEY;
        }

        /// <summary>
        /// 执行脚本
        /// </summary>
        protected string Eval(string expression, Dictionary<string, object> formData, string taskId, List<string> nodeNames)
        {
            var funcData = FilterParmMatch.CurrentParmMatch.ToIgnoreCaseDictionary();
            var nodes = GetNodes(taskId, nodeNames);
            foreach (var nodeName in nodeNames)
            {
                var key = $"{{{nodeName}}}";
                if (!expression.Contains(key)) continue;
                var group = nodes.Where(c => c.NODE_NAME == nodeName).ToList();
                funcData.Add(key, () => group.Count < 1 ? 0 : group.Count(c => c.NODE_STATUS == NodeStatus.Agree) * 1.0 / group.Count);
            }
            if (expression.Contains("{通过率}"))//参与审批人数的通过率（同意的人数/总人数）
            {
                funcData.Add("{通过率}", () =>
                {
                    if (nodeNames.Exists(c => !nodes.Any(a => a.NODE_NAME == c))) return 0;//有节点还没走到,通过率就算0
                    return nodes.Count < 1 ? 0 : nodes.Count(c => c.NODE_STATUS == NodeStatus.Agree) * 1.0 / nodes.Count;
                });
            }
            if (expression.Contains("{节点通过率}"))//任务节点的通过率（走过的上级节点数/上级总节点数）
            {
                funcData.Add("{节点通过率}", () =>
                {
                    if (nodeNames.Count < 1) return 0;//有节点还没走到,通过率就算0
                    return nodes.Where(c => c.NODE_STATUS == NodeStatus.Agree).Select(c => c.NODE_NAME).Distinct().Count() * 1.0 / nodeNames.Count;
                });
            }
            return expression.Eval(formData, funcData).CastTo<string>();
        }

        protected List<WF_NODE> GetNodes(string taskId, List<string> nodeNames)
        {
            if (nodeNames.Count < 1) return new List<WF_NODE>();
            return _dbContext.Query<WF_NODE>().Where(c => c.TASK_ID == taskId && nodeNames.Contains(c.NODE_NAME))
                .Where(WF_NODEExtensions.PassRationFilter).Select(c => new WF_NODE()
                {
                    NODE_NAME = c.NODE_NAME,
                    NODE_STATUS = c.NODE_STATUS
                }).ToList();
        }

        /// <summary>
        /// 设置Bpmn节点信息让拦截器调用
        /// </summary>
        protected void SetBpmnNodeInfo()
        {
            if (_info == null || _info.CurrentNode?.Id == Id)
                return;
            _info.CurrentNode = new()
            {
                Id = Id,
                Name = Name,
                Title = Title,
                BpmnType = BpmnType,
                Properties = Properties
            };
        }

        /// <summary>
        /// 执行当前模型
        /// </summary>
        public abstract Task Execute();

        /// <summary>
        /// 完成当前节点
        /// </summary>
        public abstract Task Complate();
    }
}