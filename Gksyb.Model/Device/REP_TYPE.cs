using Chloe.Annotations;
using System;
using System.ComponentModel;
using System.Data;

namespace Gksyb.Model
{
    /// <summary>
    /// 实体类REP_TYPE
    /// </summary>
    [Table("REP_TYPE")]
    public class REP_TYPE
    {

        /// <summary>
        /// 故障分类名称
        /// </summary>
        [Description("故障分类名称")]
        [Column(DbType = DbType.AnsiString)]
        public string REP_TYPE_NAME { get; set; }

        /// <summary>
        /// 编辑人
        /// </summary>
        [Description("编辑人")]
        [Column(DbType = DbType.AnsiString)]
        public string EDIT_USER { get; set; }

        /// <summary>
        /// 编辑人ID
        /// </summary>
        [Description("编辑人ID")]
        [Column(DbType = DbType.AnsiString)]
        public string EDIT_USERID { get; set; }

        /// <summary>
        /// 编辑日期
        /// </summary>
        [Description("编辑日期")]
        public DateTime? EDIT_DATE { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [Description("备注")]
        [Column(DbType = DbType.AnsiString)]
        public string MEMO { get; set; }

        /// <summary>
        /// 主键
        /// </summary>
        [Description("主键")]
        [Column(IsPrimaryKey = true, DbType = DbType.AnsiString)]
        public string REP_TYPE_ID { get; set; }

        /// <summary>
        /// 添加人ID
        /// </summary>
        [Description("添加人ID")]
        [Column(DbType = DbType.AnsiString)]
        public string CREATE_USERID { get; set; }

        /// <summary>
        /// 添加时间
        /// </summary>
        [Description("添加时间")]
        public DateTime? CREATEDATE { get; set; }

        /// <summary>
        /// 修改人ID
        /// </summary>
        [Description("修改人ID")]
        [Column(DbType = DbType.AnsiString)]
        public string MODIFY_USERID { get; set; }

        /// <summary>
        /// 修改时间
        /// </summary>
        [Description("修改时间")]
        public DateTime? MODIFYDATE { get; set; }

    }
}