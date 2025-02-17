namespace Gksyb.Core.Interfaces.WorkFlow
{
    public class FlowEventInfo
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
        /// 流程名称
        /// </summary>
        public string FlowName { get; set; }

        /// <summary>
        /// 所属组 可用于判断是否对接第三方平台
        /// </summary>
        public string FlowGroup { get; set; }

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
        public Dictionary<string, object> FormData { get; set; }

        /// <summary>
        /// 发起人
        /// </summary>
        public string Creator { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// 程序名
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// 节点信息
        /// </summary>
        public List<NodeInfo> NodeInfos { get; set; } = new List<NodeInfo>();

        /// <summary>
        /// 根据FlowExecuteInfo返回FlowEventInfo对象
        /// </summary>
        public static FlowEventInfo FromFlowExecuteInfo(FlowExecuteInfo info) => new()
        {
            TaskId = info.TaskId,
            TaskKey = info.TaskKey,
            FlowCode = info.FlowCode,
            FlowName = info.FlowName,
            FlowGroup = info.FlowGroup,
            FlowId = info.FlowId,
            Title = info.RealTitle,
            FormData = info.FormData,
            Creator = info.Creator,
            CreateDate = info.CreateDate,
            AppName = info.AppName
        };
    }
}