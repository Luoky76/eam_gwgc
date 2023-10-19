using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using Magicodes.ExporterAndImporter.Pdf;
using System.ComponentModel.DataAnnotations;
using WkHtmlToPdfDotNet;

namespace EAM.Special.DTO
{
    public class ExportMonthTemplateData<T>
    {
        public string TABLEDATE { get; set; }
        public string DATEYEAR { get; set; }
        public List<T> List { get; set; }
        public string SHIPTOTAL { get; set; }
        public string ZYTIMETOTAL { get; set; }
        public string STOPTIMETOTAL { get; set; }
        public string DAILYCONSUMPTIONTOTAL { get; set; }
        public string MASTERTOTAL { get; set; }
        public string AUXILIARYTOTAL { get; set; }
        public string PUMPTOTAL { get; set; }
        public string LUBRICATETOTAL { get; set; }
        public string TOTAL { get; set; }
        public string DEVICE_NAME { get; set; }
    }

    [ExcelExporter(Name = "施工能耗月度报表", Author = "港务", AutoFitMaxRows = 5000)]
    [ExcelImporter(MaxCount = 50000)]
    [PdfExporter(Orientation = Orientation.Landscape, PaperKind = PaperKind.A4, IsWriteHtml = true, IsEnablePagesCount = false)]
    public class BuildMonthExportData
    {

        [ExporterHeader(DisplayName = "船名")]
        public string DEVICE_NAME { get; set; }

        [ExporterHeader(DisplayName = "船次")]
        public decimal? SHIPTIMES { get; set; }

        [ExporterHeader(DisplayName = "作业时间")]
        public decimal? ZYTIME { get; set; }

        [ExporterHeader(DisplayName = "停工时间")]
        public decimal? STOPTIME { get; set; }

        [ExporterHeader(DisplayName = "淡水总耗")]
        public decimal? DAILYCONSUMPTION { get; set; }

        [ExporterHeader(DisplayName = "主机总耗")]
        public decimal? MASTER { get; set; }

        [ExporterHeader(DisplayName = "辅机总耗")]
        public decimal? AUXILIARY { get; set; }

        [ExporterHeader(DisplayName = "泵机总耗")]
        public decimal? PUMP { get; set; }

        [ExporterHeader(DisplayName = "滑油总耗")]
        public decimal? LUBRICATE { get; set; }
    }
}