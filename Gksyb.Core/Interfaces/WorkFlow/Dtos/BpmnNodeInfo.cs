namespace Gksyb.Core.Interfaces.WorkFlow
{
    /// <summary>
    /// 流程节点信息
    /// </summary>
    public class BpmnNodeInfo
    {
        /// <summary>
        /// 标识
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Bpmn类型
        /// </summary>
        public string BpmnType { get; set; }

        /// <summary>
        /// 属性
        /// </summary>
        public Dictionary<string, object> Properties { get; set; }

        /// <summary>
        /// 根据名称获取属性
        /// </summary>
        public T GetProperties<T>(string name)
        {
            if (Properties == null) return default;
            if (!Properties.ContainsKey(name)) return default;
            return Properties[name].CastTo<T>(default);
        }
    }
}