using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using Magicodes.ExporterAndImporter.Pdf;
using System;
using System.ComponentModel.DataAnnotations;

namespace EAM.Device.services.Dto
{
    [ExcelExporter(Name = "通用导出测试", Author = "港口事业部", AutoFitMaxRows = 5000)]
    [ExcelImporter(MaxCount = 50000)]
    public class PmImportDto
    {
        /// <summary>
        ///  序号
        /// </summary>
        [Display(Name = "序号")]
        [ImporterHeader(Name = "序号")]
        public string ID { get; set; }

        /// <summary>
        ///  编号
        /// </summary>
        [Display(Name = "编号")]
        [ImporterHeader(Name = "编号")]
        public string STD_CODE { get; set; }

        /// <summary>
        ///  船舶编号
        /// </summary>
        [Display(Name = "船舶编号")]
        [ImporterHeader(Name = "船舶编号")]
        public string DEVICE_CODE { get; set; }

        /// <summary>
        ///  船舶名称
        /// </summary>
        [Display(Name = "船舶名称")]
        [ImporterHeader(Name = "船舶名称")]
        public string DEVICE_NAME { get; set; }

        /// <summary>
        ///  维保项目
        /// </summary>
        [Display(Name = "维保项目")]
        [ImporterHeader(Name = "维保项目")]
        public string PART_NAME { get; set; }

        /// <summary>
        ///  维保内容
        /// </summary>
        [Display(Name = "维保内容")]
        [ImporterHeader(Name = "维保内容")]
        public string CONTENT { get; set; }

        /// <summary>
        ///  执行人
        /// </summary>
        [Display(Name = "执行人")]
        [ImporterHeader(Name = "执行人")]
        public string EXE_USER { get; set; }

        /// <summary>
        ///  检查人
        /// </summary>
        [Display(Name = "检查人")]
        [ImporterHeader(Name = "检查人")]
        public string CHK_USER { get; set; }

        /// <summary>
        ///  周期
        /// </summary>
        [Display(Name = "周期(只能填每周，月度，季度，半年，年度，2.5年，5年)")]
        [ImporterHeader(Name = "周期(只能填每周，月度，季度，半年，年度，2.5年，5年)")]
        public string CYCLE { get; set; }

        /// <summary>
        ///  部门
        /// </summary>
        [Display(Name = "部门(甲板部或机舱部)")]
        [ImporterHeader(Name = "部门(甲板部或机舱部)")]
        public string DEPARTMENT { get; set; }

        /// <summary>
        ///  备注
        /// </summary>
        [Display(Name = "备注")]
        [ImporterHeader(Name = "备注")]
        public string MEMO { get; set; }

        /// <summary>
        ///  是否附件判断
        /// </summary>
        [Display(Name = "是否附件判断（填是或者否）")]
        [ImporterHeader(Name = "是否附件判断（填是或者否）")]
        public string IS_ATTACH { get; set; }

    }
}
