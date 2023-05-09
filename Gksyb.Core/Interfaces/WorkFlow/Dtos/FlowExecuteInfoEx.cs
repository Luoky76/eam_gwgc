using Gksyb.Core.Interfaces.Auth;
using Newtonsoft.Json;

namespace Gksyb.Core.Interfaces.WorkFlow
{
    /// <summary>
    /// 流程节点执行信息
    /// </summary>
    public partial class FlowExecuteInfo
    {
        /// <summary>
        /// 流程名称
        /// </summary>
        [JsonIgnore]
        public string FlowName { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        [JsonIgnore]
        public string Title { get; set; }

        /// <summary>
        /// 目标节点
        /// </summary>
        [JsonIgnore]
        public string ToNode { get; set; }

        /// <summary>
        /// 是否拥有目标节点,简化写法
        /// </summary>
        [JsonIgnore]
        public bool ToNodeIsEmpty => string.IsNullOrWhiteSpace(ToNode);

        /// <summary>
        /// 下一节点处理人
        /// </summary>
        [JsonIgnore]
        public List<UserInfo> Users = null;
    }
}