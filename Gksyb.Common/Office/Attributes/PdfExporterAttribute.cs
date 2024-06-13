using WkHtmlToPdfDotNet;

namespace Gksyb.Common.Office.Core
{
    /// <summary>
    /// PDF导出特性
    /// </summary>
    public class PdfExporterAttribute : ExporterAttribute
    {
        /// <summary>
        /// 方向
        /// </summary>
        public Orientation Orientation { get; set; } = Orientation.Landscape;

        /// <summary>
        /// 纸张类型（默认A4，必须）
        /// </summary>
        public PaperKind PaperKind { get; set; } = PaperKind.A4;

        /// <summary>
        /// 是否启用分页数
        /// </summary>
        public bool IsEnablePagesCount { get; set; }

        /// <summary>
        /// 是否输出HTML模板
        /// </summary>
        public bool IsWriteHtml { get; set; }

        /// <summary>
        /// 头部设置
        /// </summary>
        public HeaderSettings HeaderSettings { get; set; }

        /// <summary>
        /// 底部设置
        /// </summary>
        public FooterSettings FooterSettings { get; set; }

        /// <summary>
        /// 边距设置
        /// </summary>
        public MarginSettings MarginSettings { get; set; }

        /// <summary>
        /// 纸张大小（仅在PaperKind=custom下生效）
        /// </summary>
        public PechkinPaperSize PaperSize { get; set; }
    }
}