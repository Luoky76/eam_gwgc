namespace Gksyb.Workflow.Services.Workflow.Dtos
{
    /// <summary>
    /// 流程图形数据
    /// </summary>
    public class FlowGraphData
    {
        /// <summary>
        /// 节点
        /// </summary>
        public List<FlowGraphNode> Nodes { get; set; }

        /// <summary>
        /// 连线
        /// </summary>
        public List<FlowGraphEdge> Edges { get; set; }
    }

    /// <summary>
    /// 流程节点图形数据
    /// </summary>
    public class FlowGraphNode
    {
        /// <summary>
        /// 节点ID
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// 节点类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string ServiceName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Type)) return string.Empty;
                return $"Bpmn{Type.Replace("bpmn:", "").ToTitleCase()}Service";
            }
        }

        /// <summary>
        /// 节点扩展属性
        /// </summary>
        public Dictionary<string, object> Properties { get; set; }

        /// <summary>
        /// 文本属性
        /// </summary>
        public Dictionary<string, object> Text { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title
        {
            get
            {
                return GetText("value") as string;
            }
        }

        /// <summary>
        /// 获取文本属性
        /// </summary>
        private object GetText(string name)
        {
            if (Text == null) return null;
            if (!Text.ContainsKey(name)) return null;
            return Text[name];
        }
    }

    /// <summary>
    /// 流程边图形数据
    /// </summary>
    public class FlowGraphEdge : FlowGraphNode
    {
        /// <summary>
        /// 来源节点
        /// </summary>
        public string SourceNodeId { get; set; }

        /// <summary>
        /// 目标节点
        /// </summary>
        public string TargetNodeId { get; set; }
    }
}