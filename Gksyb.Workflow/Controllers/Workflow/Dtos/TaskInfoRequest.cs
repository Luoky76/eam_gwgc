namespace Gksyb.Workflow.Controllers.Workflow.Dtos
{
    public class TaskInfoRequest
    {
        /// <summary>
        /// 节点ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 任务ID
        /// </summary>
        public string TaskId { get; set; }

        /// <summary>
        /// 流程ID
        /// </summary>
        public string FlowId { get; set; }

        /// <summary>
        /// 流程编码
        /// </summary>
        public string FlowCode { get; set; }

        /// <summary>
        /// 重新发起的任务ID
        /// </summary>
        public string CopyTaskId { get; set; }

        /// <summary>
        /// 启用新流程
        /// </summary>
        public bool DoStartFlow => string.IsNullOrWhiteSpace(Id) && string.IsNullOrWhiteSpace(TaskId);
    }
}