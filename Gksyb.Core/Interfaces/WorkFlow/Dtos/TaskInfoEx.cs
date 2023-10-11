namespace Gksyb.Core.Interfaces.WorkFlow
{
    public class TaskInfoEx : TaskInfo
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 流程内容
        /// </summary>
        public string FlowContent { get; set; }

        /// <summary>
        /// 表单内容
        /// </summary>
        public string FormContent { get; set; }

        /// <summary>
        /// 表单url
        /// </summary>
        public string FormUrl { get; set; }

        /// <summary>
        /// 表单url
        /// </summary>
        public string FormMobileUrl { get; set; }

        /// <summary>
        /// 节点ID
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// 作业节点ID
        /// </summary>
        public string WorkNodeId { get; set; }

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
    }
}