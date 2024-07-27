using Gksyb.Workflow.Services.Workflow.Bpmn;

namespace Gksyb.Workflow.Services.Workflow.Dtos
{
    public class FlowBpmnData
    {
        /// <summary>
        /// 任务节点
        /// </summary>
        public List<BpmnNodeService> Nodes { get; set; } = new List<BpmnNodeService>();

        /// <summary>
        /// 变迁
        /// </summary>
        public List<BpmnSequenceFlowService> Sequences { get; set; } = new List<BpmnSequenceFlowService> { };
    }
}