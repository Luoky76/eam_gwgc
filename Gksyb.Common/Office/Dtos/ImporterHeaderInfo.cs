using System.Reflection;

namespace Gksyb.Common.Office.Core
{
    /// <summary>
    /// 导入列头设置
    /// </summary>
    public class ImporterHeaderInfo
    {
        /// <summary>
        /// 是否必填
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// 列名称
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// 列属性
        /// </summary>
        public ImporterHeaderAttribute Header { get; set; }
        /// <summary>
        /// 图属性
        /// </summary>
        public ImportImageFieldAttribute ImportImageFieldAttribute { get; set; }

        /// <summary>
        /// </summary>
        public Dictionary<string, dynamic> MappingValues { get; set; } = new Dictionary<string, dynamic>();

        /// <summary>
        /// 是否存在
        /// </summary>
        public bool IsExist { get; set; }

        /// <summary>
        /// 属性信息
        /// </summary>
        public PropertyInfo PropertyInfo { get; set; }
    }
}