using System.Reflection;

namespace Gksyb.Common.Office.Core
{
    /// <summary>
    /// 值映射
    /// </summary>
    public abstract class ValueMappingsBaseAttribute : Attribute
    {
        /// <summary>
        /// 根据字段信息获取映射
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public abstract Dictionary<string, object> GetMappings(PropertyInfo propertyInfo);
    }
}