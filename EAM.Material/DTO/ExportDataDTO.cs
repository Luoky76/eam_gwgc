using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using Magicodes.ExporterAndImporter.Pdf;
using System.ComponentModel.DataAnnotations;
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
}