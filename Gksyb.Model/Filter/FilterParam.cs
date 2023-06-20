using System.Data;

namespace Gksyb.Model.Filter
{
    /// <summary>
    /// 过滤变量
    /// </summary>
    public class FilterParam
    {
        public FilterParam(string name, object value, DbType? dbType = null)
        {
            this.Name = name;
            this.Value = value;
            this.DbType = dbType;
        }

        public string Name { get; set; }
        public object Value { get; set; }
        public DbType? DbType { get; set; }
    }
}