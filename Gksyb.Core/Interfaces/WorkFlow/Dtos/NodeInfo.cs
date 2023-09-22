namespace Gksyb.Core.Interfaces.WorkFlow.Dtos
{
    public class NodeInfo
    {
        /// <summary>
        /// 节点Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 节点编号
        /// </summary>
        public string NodeId { get; set; }

        /// <summary>
        /// 节点名称
        /// </summary>
        public string NodeName { get; set; }

        /// <summary>
        /// 节点标题
        /// </summary>
        public string NodeTitle { get; set; }

        /// <summary>
        /// 节点类型
        /// </summary>
        public string NodeType { get; set; }

        /// <summary>
        /// 节点状态 参考<see cref="NodeStatus"/>
        /// </summary>
        public int? NodeStatus { get; set; }

        /// <summary>
        /// 节点处理人ID
        /// </summary>
        public long? NodeUserId { get; set; }

        /// <summary>
        /// 节点处理人
        /// </summary>
        public string NodeUserName { get; set; }

        /// <summary>
        /// 节点处理人
        /// </summary>
        public string NodeUser { get; set; }

        /// <summary>
        /// 处理意见
        /// </summary>
        public string NodeReason { get; set; }

        /// <summary>
        /// 查看日期
        /// </summary>
        public DateTime? Viewdate { get; set; }

        /// <summary>
        /// 完成日期
        /// </summary>
        public DateTime? Finishdate { get; set; }
    }
}