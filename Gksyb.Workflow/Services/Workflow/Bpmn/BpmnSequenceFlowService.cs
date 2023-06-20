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

        public void Init(FlowGraphEdge flowGraphEdge, List<BpmnNodeService> services)
        {
            Init(flowGraphEdge);
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

        public override async Task Execute(FlowExecuteInfo info)
        {
            if (Target == null) return;
            var expression = Expression;
            if (!string.IsNullOrWhiteSpace(expression))
            {
                if (info.FormData == null)
                {
                    await SetFormData(info);
                }
                var result = Eval(expression, info.FormData, info.TaskId, new List<string>() { Source.Name }) == "True";
                if (!result) return;
            }
            await Target.Execute(info);
        }

        public override async Task Complate(FlowExecuteInfo info)
        {
            await Task.CompletedTask;
            throw new MessageException($"连线{Title}不能被完成");
        }
    }
}