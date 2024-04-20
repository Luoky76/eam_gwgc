using Gksyb.Common.Office.Core;
using Gksyb.Common.Office.Html;
using System.Text;
using WkHtmlToPdfDotNet;

namespace Gksyb.Common.Office.Pdf
{
    /// <summary>
    /// Pdf导出逻辑
    /// </summary>
    public class PdfExporter : IExportFileByTemplate
    {
        private static readonly SynchronizedConverter PdfConverter = null;

        static PdfExporter()
        {
            PdfConverter = new(new PdfTools());
        }

        private readonly HtmlExporter _htmlExporter;

        public PdfExporter()
        {
            _htmlExporter = new HtmlExporter();
        }

        /// <inheritdoc/>
        public async Task<byte[]> ExportBytesByTemplate<T>(T data, string template) where T : class
        {
            var exporterAttribute = GetExporterAttribute<T>();
            var htmlString = await _htmlExporter.ExportByTemplate(data, template);
            return ExportPdf(exporterAttribute, htmlString);
        }

        /// <inheritdoc/>
        public async Task<byte[]> ExportBytesByTemplate(object data, string template, Type type)
        {
            var exporterAttribute = GetExporterAttribute(type);
            var htmlString = await _htmlExporter.ExportByTemplate(data, template, type);
            return ExportPdf(exporterAttribute, htmlString);
        }

        /// <inheritdoc/>
        public async Task<byte[]> ExportListBytesByTemplate<T>(ICollection<T> data, string template) where T : class
        {
            var exporterAttribute = GetExporterAttribute<T>();
            var htmlString = await _htmlExporter.ExportListByTemplate(data, template);
            return ExportPdf(exporterAttribute, htmlString);
        }

        /// <inheritdoc/>
        public async Task<byte[]> ExportListBytesByTemplate<T>(ICollection<T> data, PdfExporterAttribute pdfExporterAttribute, string template) where T : class
        {
            var htmlString = await _htmlExporter.ExportListByTemplate(data, template);
            return ExportPdf(pdfExporterAttribute, htmlString);
        }

        /// <inheritdoc/>
        public async Task<byte[]> ExportBytesByTemplate<T>(T data, PdfExporterAttribute pdfExporterAttribute, string template) where T : class
        {
            var htmlString = await _htmlExporter.ExportByTemplate(data, template);
            return ExportPdf(pdfExporterAttribute, htmlString);
        }

        /// <summary>
        /// html导出pdf
        /// </summary>
        /// <param name="pdfExporterAttribute">pdf导出属性</param>
        /// <param name="htmlString">html内容</param>
        private static byte[] ExportPdf(PdfExporterAttribute pdfExporterAttribute, string htmlString)
        {
            var objSettings = new ObjectSettings
            {
                HtmlContent = htmlString,
                Encoding = Encoding.UTF8,
                PagesCount = pdfExporterAttribute.IsEnablePagesCount ? true : null,
                WebSettings = { DefaultEncoding = Encoding.UTF8.BodyName }
            };
            if (pdfExporterAttribute.HeaderSettings != null)
                objSettings.HeaderSettings = pdfExporterAttribute.HeaderSettings;

            if (pdfExporterAttribute.FooterSettings != null)
                objSettings.FooterSettings = pdfExporterAttribute.FooterSettings;

            var document = new HtmlToPdfDocument
            {
                GlobalSettings =
                {
                    PaperSize = pdfExporterAttribute.PaperKind == PaperKind.Custom? pdfExporterAttribute.PaperSize : pdfExporterAttribute.PaperKind,
                    Orientation = pdfExporterAttribute.Orientation,
                    ColorMode = ColorMode.Color,
                    DocumentTitle = pdfExporterAttribute.Name
                },
                Objects = { objSettings }
            };

            if (pdfExporterAttribute.MarginSettings != null)
                document.GlobalSettings.Margins = pdfExporterAttribute.MarginSettings;

            return PdfConverter.Convert(document);
        }

        /// <summary>
        /// 获取全局导出定义
        /// </summary>
        private static PdfExporterAttribute GetExporterAttribute<T>() where T : class
            => GetExporterAttribute(typeof(T));

        /// <summary>
        ///		 获取全局导出定义
        /// </summary>
        private static PdfExporterAttribute GetExporterAttribute(Type type)
        {
            var exporterTableAttribute = type.GetAttribute<PdfExporterAttribute>(true);
            if (exporterTableAttribute != null)
                return exporterTableAttribute;

            var export = type.GetAttribute<ExporterAttribute>(true) ?? new PdfExporterAttribute();
            return new PdfExporterAttribute
            {
                FontSize = export.FontSize,
                HeaderFontSize = export.HeaderFontSize
            };
        }
    }
}