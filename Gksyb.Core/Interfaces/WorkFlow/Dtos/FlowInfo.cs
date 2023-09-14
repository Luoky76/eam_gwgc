namespace Gksyb.Core.Interfaces.WorkFlow
{
    /// <summary>
    /// 流程定义
    /// </summary>
    public class FlowInfo
    {
        /// <summary>
        /// 主键
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 流程名称
        /// </summary>
        public string FlowName { get; set; }

        /// <summary>
        /// 所属组
        /// </summary>
        public string FlowGroup { get; set; }

        /// <summary>
        /// 流程标题
        /// </summary>
        public string FlowTitle { get; set; }

        /// <summary>
        /// 流程序号
        /// </summary>
        public string FlowOrder { get; set; }

        /// <summary>
        /// 流程表单url
        /// </summary>
        public string FlowFormUrl { get; set; }

        /// <summary>
        /// 流程表单url
        /// </summary>
        public string FlowFormMobileUrl { get; set; }

        /// <summary>
        /// 流程版本
        /// </summary>
        public int? FlowVersion { get; set; }

        /// <summary>
        /// 所属组织
        /// </summary>
        public string Corpid { get; set; }
    }
}