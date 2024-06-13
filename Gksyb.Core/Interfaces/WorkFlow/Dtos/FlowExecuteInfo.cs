namespace Gksyb.Core.Interfaces.WorkFlow
{
    /// <summary>
    /// 流程节点执行信息
    /// </summary>
    public partial class FlowExecuteInfo
    {
        /// <summary>
        /// 节点主键
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 任务ID
        /// </summary>
        public string TaskId { get; set; }

        /// <summary>
        /// 任务主键（默认为表单主键，获取不到则取<seealso cref="TaskId"/>）
        /// </summary>
        public string TaskKey { get; set; }

        /// <summary>
        /// 流程编码
        /// </summary>
        public string FlowCode { get; set; }

        /// <summary>
        /// 流程主键
        /// </summary>
        public string FlowId { get; set; }

        /// <summary>
        /// 表单数据
        /// </summary>
        public Dictionary<string, object> FormData { get; set; }

        /// <summary>
        /// 节点ID
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// 节点状态 <seealso cref="WorkFlow.NodeStatus"/>
        /// </summary>
        public int? NodeStatus { get; set; }

        /// <summary>
        /// 节点处理意见
        /// </summary>
        public string NodeReason { get; set; }

        /// <summary>
        /// 公司ID
        /// </summary>
        public string CorpId { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        public string Operators { get; set; }
    }
}