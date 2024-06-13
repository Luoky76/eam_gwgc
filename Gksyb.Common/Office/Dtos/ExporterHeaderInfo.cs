using System.Reflection;

namespace Gksyb.Common.Office.Core
{
    /// <summary>
    /// 导出列头部信息
    /// </summary>
    public class ExporterHeaderInfo
    {
        /// <summary>
        /// 列索引（自动）
        /// 如需排序，请设置ExporterHeaderAttribute.ColumnIndex 属性
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// 列名称
        /// </summary>
        public string PropertyName { get; set; }

        public PropertyInfo PropertyInfo { get; set; }

        /// <summary>
        /// 列属性
        /// </summary>
        public ExporterHeaderAttribute ExporterHeaderAttribute { get; set; }

        /// <summary>
        /// 图片属性
        /// </summary>
        public ExportImageFieldAttribute ExportImageFieldAttribute { get; set; }

        /// <summary>
        /// C#数据类型
        /// </summary>
        public Type CsTypeName { get; set; }

        /// <summary>
        /// 最终显示的列名
        /// </summary>
        public string DisplayName { set; get; }

        /// <summary>
        /// </summary>
        public Dictionary<string, dynamic> MappingValues { get; set; } = new Dictionary<string, dynamic>();
    }
}