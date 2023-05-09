namespace Gksyb.Workflow.Controllers.Workflow.Dtos
{
    /// <summary>
    /// 节点信息
    /// </summary>
    public class NodeInfo
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 流程标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string NodeName { get; set; }

        /// <summary>
        /// 节点状态
        /// </summary>
        public int? NodeStatus { get; set; }

        /// <summary>
        /// 节点处理意见
        /// </summary>
        public string NodeReason { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// 启动时间
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 查看时间
        /// </summary>
        public DateTime? ViewDate { get; set; }

        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? FinishDate { get; set; }

        /// <summary>
        /// 任务ID
        /// </summary>
        public string TaskId { get; set; }

        /// <summary>
        /// 发起人
        /// </summary>
        public string Creator { get; set; }

        /// <summary>
        /// 发起时间
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// 任务完成标志
        /// </summary>
        public string TaskFinishFlag { get; set; }

        /// <summary>
        /// 任务完成时间
        /// </summary>
        public DateTime? TaskFinishDate { get; set; }
    }
}