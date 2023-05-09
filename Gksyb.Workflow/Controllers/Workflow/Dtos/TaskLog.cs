namespace Gksyb.Workflow.Controllers.Workflow.Dtos
{
    public class TaskLog
    {
        /// <summary>
        /// 节点主键
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public string OperType { get; set; }

        /// <summary>
        /// 操作标题
        /// </summary>
        public string OperTitle { get; set; }

        /// <summary>
        /// 操作明细
        /// </summary>
        public string OperDetail { get; set; }

        /// <summary>
        /// 操作日期
        /// </summary>
        public DateTime? OperDate { get; set; }
    }
}