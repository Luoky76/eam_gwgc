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

        public BpmnBaseService(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected void Init(FlowGraphNode flowGraphNode)
        {
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
        protected async Task SetFormData(FlowExecuteInfo info)
        {
            var formData = await _dbContext.Query<WF_TASK>().Where(c => c.ID == info.TaskId).Select(c => c.FLOW_FORM_DATA).FirstOrDefaultAsync();
            info.FormData = (formData ?? "").ToObject<Dictionary<string, object>>();
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
            if (expression.Contains("{通过率}"))
            {
                funcData.Add("{通过率}", () =>
                {
                    if (nodeNames.Exists(c => !nodes.Any(a => a.NODE_NAME == c))) return 0;//有节点还没走到,通过率就算0
                    return nodes.Count < 1 ? 0 : nodes.Count(c => c.NODE_STATUS == NodeStatus.Agree) * 1.0 / nodes.Count;
                });
            }
            if (expression.Contains("{节点通过率}"))
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
        /// 执行当前模型
        /// </summary>
        public abstract Task Execute(FlowExecuteInfo info);

        /// <summary>
        /// 完成当前节点
        /// </summary>
        public abstract Task Complate(FlowExecuteInfo info);
    }
}