using Gksyb.Core.Interfaces.WorkFlow;
using Gksyb.Workflow.Services.Workflow.Dtos;

namespace Gksyb.Workflow.Services.Workflow.Bpmn
{
    /// <summary>
    /// 变迁服务
    /// </summary>
    [ServiceLifetime]
    public class BpmnSequenceFlowService : BpmnBaseService, IBaseService
    {
        public BpmnSequenceFlowService(IDbContext dbContext) : base(dbContext)
        {
        }

        public void Init(FlowExecuteInfo _info, FlowGraphEdge flowGraphEdge, List<BpmnNodeService> services)
        {
            Init(_info, flowGraphEdge);
            Source = services.FirstOrDefault(c => c.Id == flowGraphEdge.SourceNodeId);
            if (Source != null && !Source.Outputs.Contains(this)) Source.Outputs.Add(this);
            Target = services.FirstOrDefault(c => c.Id == flowGraphEdge.TargetNodeId);
            if (Target != null && !Target.Inputs.Contains(this)) Target.Inputs.Add(this);
        }

        /// <summary>
        /// 变迁的源节点
        /// </summary>
        public BpmnNodeService Source { get; set; }

        /// <summary>
        /// 变迁的目标节点
        /// </summary>
        public BpmnNodeService Target { get; set; }

        /// <summary>
        /// 变迁的条件表达式
        /// </summary>
        public string Expression
        {
            get
            {
                return GetProperties("expr") as string;
            }
        }

        public override async Task Execute()
        {
            if (Target == null) return;
            SetBpmnNodeInfo();
            var expression = Expression;
            if (!string.IsNullOrWhiteSpace(expression))
            {
                if (_info.FormData == null)
                {
                    await SetFormData();
                }
                var result = Eval(expression, _info.FormData, _info.TaskId, new List<string>() { Source.Name }) == "True";
                if (!result) return;
            }
            await Target.Execute();
        }

        public override async Task Complate()
        {
            await Task.CompletedTask;
            throw new MessageException($"连线{Title}不能被完成");
        }
    }
}