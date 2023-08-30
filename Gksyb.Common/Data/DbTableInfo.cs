namespace Gksyb.Common.Data
{
    public class DbTableInfo
    {
        /// <summary>
        /// 模块
        /// </summary>
        public string Module { get; set; }

        /// <summary>
        /// 表名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// 类型（table，view）
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 表空间
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// 数据源
        /// </summary>
        public string DataSource { get; set; }

        /// <summary>
        /// 列信息
        /// </summary>
        public List<DbColumnInfo> Columns { get; set; }
    }
}