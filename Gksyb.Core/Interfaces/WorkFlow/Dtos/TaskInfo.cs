namespace Gksyb.Core.Interfaces.WorkFlow
{
    public class TaskInfo
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public string TaskId { get; set; }

        /// <summary>
        /// 任务主键
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
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 表单数据
        /// </summary>
        public string FormData { get; set; }

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

        /// <summary>
        /// 流转节点信息
        /// </summary>
        public List<NodeInfo> Nodes { get; set; }
    }
}