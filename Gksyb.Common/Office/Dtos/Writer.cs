namespace Gksyb.Common.Office.Core
{
    /// <summary>
    /// 写入器
    /// </summary>
    internal class Writer
    {
        /// <summary>
        /// 地址
        /// </summary>
        public string TplAddress { get; set; }

        /// <summary>
        /// 单元格原始字符串
        /// </summary>
        public string CellString { get; set; }

        /// <summary>
        /// 写入器类型
        /// </summary>
        public WriterTypes WriterType { get; set; }

        /// <summary>
        /// 表格数据对象Key
        /// </summary>
        public string TableKey { get; set; }

        /// <summary>
        /// 行号
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// 列号
        /// </summary>
        public int ColIndex { get; set; }

        /// <summary>
        /// 单元格脚本
        /// </summary>
        public List<ScriptInfo> CellScript { get; set; }
    }

    /// <summary>
    /// 写入器类型
    /// </summary>
    internal enum WriterTypes
    {
        /// <summary>
        /// 单元格
        /// </summary>
        Cell,

        /// <summary>
        /// 表格
        /// </summary>
        Table
    }

    /// <summary>
    /// 脚本信息
    /// </summary>
    internal class ScriptInfo
    {
        /// <summary>
        /// 类型 image formula
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 变量名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 内容体
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 参数部分
        /// </summary>
        public string Params { get; set; }
    }

    internal class ParamsInfo
    {
        /// <summary>
        /// 类型 image formula
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 变量名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 内容体
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 参数部分
        /// </summary>
        public string Params { get; set; }
    }
}