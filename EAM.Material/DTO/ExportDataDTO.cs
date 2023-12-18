using Chloe.Annotations;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using Magicodes.ExporterAndImporter.Pdf;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using WkHtmlToPdfDotNet;

namespace EAM.Material.DTO
{
    public class ExportTemplateData<T>
    {
        public string TABLEDATE { get; set; }
        public string DATEYEAR { get; set; }
        public string TOTAL { get; set; }
        public List<T> List { get; set; }
    }

    [ExcelExporter(Name = "采购订单年度报表", Author = "港务", AutoFitMaxRows = 5000)]
    [ExcelImporter(MaxCount = 50000)]
    [PdfExporter(Orientation = Orientation.Landscape, PaperKind = PaperKind.A4, IsWriteHtml = true, IsEnablePagesCount = false)]
    public class OrderExportData
    {

        [ExporterHeader(DisplayName = "申请单号")]
        public string ORDER_CODE { get; set; }

        [ExporterHeader(DisplayName = "订单类型")]
        public string ORDER_TYPE { get; set; }

        [ExporterHeader(DisplayName = "下单时间")]
        public DateTime? ORDER_DATE { get; set; }

        [ExporterHeader(DisplayName = "下单日期")]
        public string ORDER_DATESTR { get; set; }

        [ExporterHeader(DisplayName = "申购公司")]
        public string DEPT_NAME { get; set; }

        [ExporterHeader(DisplayName = "订单金额（元）")]
        public decimal? ORDER_MONEY { get; set; }

        [ExporterHeader(DisplayName = "供应商")]
        public string PROVIDER_NAME { get; set; }
    }


    [ExcelExporter(Name = "申请物资", Author = "港务", AutoFitMaxRows = 5000)]
    [ExcelImporter(MaxCount = 50000)]
    [PdfExporter(Orientation = Orientation.Landscape, PaperKind = PaperKind.A4, IsWriteHtml = true, IsEnablePagesCount = false)]
    public class SpExportData
    {
        [Display(Name = "物料名称")]
        [Description("物料名称")]
        [Column(DbType = DbType.AnsiString)]
        public string SP_NAME { get; set; }

        [Display(Name = "型号规格")]
        [Description("型号规格")]
        [Column(DbType = DbType.AnsiString)]
        public string SP_SIZE { get; set; }

        [Display(Name = "计量单位")]
        [Description("计量单位")]
        [Column(DbType = DbType.AnsiString)]
        public string UNIT { get; set; }

        [Display(Name = "品牌、厂家")]
        [Description("品牌、厂家")]
        [Column(DbType = DbType.AnsiString)]
        public string PRODUCE { get; set; }

        [Display(Name = "申请数量")]
        [Description("申请数量")]
        [Column(DbType = DbType.Decimal)]
        public decimal? COUNT { get; set; }

        [Display(Name = "备注")]
        [Description("备注")]
        [Column(DbType = DbType.AnsiString)]
        public string MEMO { get; set; }

    }

    [ExcelExporter(Name = "申请物资", Author = "港务", AutoFitMaxRows = 5000)]
    [ExcelImporter(MaxCount = 50000)]
    [PdfExporter(Orientation = Orientation.Landscape, PaperKind = PaperKind.A4, IsWriteHtml = true, IsEnablePagesCount = false)]
    public class SpDetailExportData
    {
        [Display(Name = "物料名称")]
        [Description("物料名称")]
        [Column(DbType = DbType.AnsiString)]
        public string SP_NAME { get; set; }

        [Display(Name = "型号规格")]
        [Description("型号规格")]
        [Column(DbType = DbType.AnsiString)]
        public string SP_SIZE { get; set; }

        [Display(Name = "计量单位")]
        [Description("计量单位")]
        [Column(DbType = DbType.AnsiString)]
        public string UNIT { get; set; }

        [Display(Name = "品牌、厂家")]
        [Description("品牌、厂家")]
        [Column(DbType = DbType.AnsiString)]
        public string PRODUCE { get; set; }

        [Display(Name = "物料分类")]
        [Description("物料分类")]
        [Column(DbType = DbType.AnsiString)]
        public string TYPE_NAME { get; set; }

        [Display(Name = "备注")]
        [Description("备注")]
        [Column(DbType = DbType.AnsiString)]
        public string MEMO { get; set; }

    }
}