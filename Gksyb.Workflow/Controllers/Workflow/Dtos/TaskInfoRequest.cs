namespace Gksyb.Workflow.Controllers.Workflow.Dtos
{
    public class TaskInfoRequest
    {
        /// <summary>
        /// 任务节点ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 流程ID
        /// </summary>
        public string FlowId { get; set; }

        /// <summary>
        /// 流程编码
        /// </summary>
        public string FlowCode { get; set; }
    }
}