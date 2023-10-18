namespace Gksyb.Common.Data
{
    public class DbColumnInfo
    {
        /// <summary>
        /// 所属表空间
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// 所属表
        /// </summary>
        public string Table { get; set; }

        /// <summary>
        /// 列名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 映射到 C# 类型
        /// </summary>
        public string CsType { get; set; }

        /// <summary>
        /// 数据库枚举类型int值
        /// </summary>
        public string DbType { get; set; }

        /// <summary>
        /// 最大长度
        /// </summary>
        public long? MaxLength { get; set; }

        /// <summary>
        /// 整数长度
        /// </summary>
        public int? Precision { get; set; }

        /// <summary>
        /// 小数长度
        /// </summary>
        public int? Scale { get; set; }

        /// <summary>
        /// 主键
        /// </summary>
        public bool? IsPrimary { get; set; }

        /// <summary>
        /// 自增标识
        /// </summary>
        public bool? IsIdentity { get; set; }

        /// <summary>
        /// 是否可DBNull
        /// </summary>
        public bool? IsNullable { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Comment { get; set; }

        /// <summary>
        /// 数据库默认值
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// 字段位置
        /// </summary>
        public int? Position { get; set; }
    }
}