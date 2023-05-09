namespace Gksyb.Workflow.Controllers.Workflow.Dtos
{
    public class TaskInfo
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 任务ID
        /// </summary>
        public string TaskId { get; set; }

        /// <summary>
        /// 流程主键
        /// </summary>
        public string FlowId { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 流程内容
        /// </summary>
        public string FlowContet { get; set; }

        /// <summary>
        /// 表单内容
        /// </summary>
        public string FormContent { get; set; }

        /// <summary>
        /// 表单url
        /// </summary>
        public string FormUrl { get; set; }

        /// <summary>
        /// 表单数据
        /// </summary>
        public string FormData { get; set; }

        /// <summary>
        /// 节点ID
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// 节点标题
        /// </summary>
        public string NodeTitle { get; set; }

        /// <summary>
        /// 节点类型
        /// </summary>
        public string NodeType { get; set; }

        /// <summary>
        /// 节点处理人ID
        /// </summary>
        public long? NodeUserId { get; set; }

        /// <summary>
        /// 节点状态
        /// </summary>
        public int? NodeStatus { get; set; }

        /// <summary>
        /// 查看时间
        /// </summary>
        public DateTime? ViewDate { get; set; }

        /// <summary>
        /// 发起人
        /// </summary>
        public string Creator { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// 流转节点信息
        /// </summary>
        public List<TaskLog> Logs { get; set; }
    }
}