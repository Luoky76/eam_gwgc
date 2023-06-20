namespace Gksyb.Core.Common
{
    public class QueryViewRequest
    {
        /// <summary>
        /// 视图名称
        /// </summary>
        public string ViewName { get; set; }

        /// <summary>
        /// 变量
        /// </summary>
        public IDictionary<string, object> Param { get; set; }

        /// <summary>
        /// GridJson配置附加参数
        /// </summary>
        public bool IsGridJson { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        public string Sort { get; set; }
    }
}