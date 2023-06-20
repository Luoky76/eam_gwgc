using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using Magicodes.ExporterAndImporter.Html;
using Magicodes.ExporterAndImporter.Pdf;
using Magicodes.ExporterAndImporter.Word;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.IO;

namespace Gksyb.Common.Office
{
    /// <summary>
    /// 文件导出
    /// </summary>
    public static class FileExport
    {
        /// <summary>
        /// DataTable导出excle
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="fileName">导出文件名</param>
        /// <param name="exporter">导出程序</param>
        /// <returns></returns>
        public static async Task<FileResult> ExportToExcel(DataTable data, string fileName = null, ExcelExporter exporter = null)
        {
            exporter ??= new ExcelExporter();
            var content = await exporter.ExportAsByteArray(data);
            return GetFileResult(content, fileName ?? $"{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        /// <summary>
        /// list导出excle
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="data">数据</param>
        /// <param name="fileName">导出文件名</param>
        /// <param name="exporter">导出程序</param>
        /// <returns></returns>
        public static async Task<FileResult> ExportToExcel<T>(ICollection<T> data, string fileName = null, ExcelExporter exporter = null) where T : class, new()
        {
            exporter ??= new ExcelExporter();
            var content = await exporter.ExportAsByteArray(data);
            return GetFileResult(content, fileName ?? $"{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        /// <summary>
        /// 导出excle表头
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="type">数据</param>
        /// <param name="fileName">导出文件名</param>
        /// <param name="exporter">导出程序</param>
        /// <returns></returns>
        public static async Task<FileResult> ExportToExcelHeader<T>(T type, string fileName = null, ExcelExporter exporter = null) where T : class, new()
        {
            exporter ??= new ExcelExporter();
            var content = await exporter.ExportHeaderAsByteArray(type);
            return GetFileResult(content, fileName ?? $"{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }

        /// <summary>
        /// 导出excel模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">数据</param>
        /// <param name="template">模板文件名</param>
        /// <param name="fileName">导出文件名</param>
        /// <returns></returns>
        public static async Task<FileResult> ExportToExcelByTemplate<T>(T type, string template, string fileName = null) where T : class, new()
        {
            return await ExportByTemplate(type, new ExcelExporter(), template, fileName);
        }

        /// <summary>
        /// 导出word模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">数据</param>
        /// <param name="template">模板文件名</param>
        /// <param name="fileName">导出文件名</param>
        /// <returns></returns>
        public static async Task<FileResult> ExportToWord<T>(T type, string template = null, string fileName = null) where T : class, new()
        {
            var templateContent = await GetTemplateContent(template);
            return await ExportByTemplate(type, new WordExporter(), templateContent, fileName);
        }

        /// <summary>
        /// 导出PDF模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">数据</param>
        /// <param name="template">模板文件名</param>
        /// <param name="fileName">导出文件名</param>
        /// <returns></returns>
        public static async Task<FileResult> ExportToPdf<T>(T type, string template = null, string fileName = null) where T : class, new()
        {
            var templateContent = await GetTemplateContent(template);
            return await ExportByTemplate(type, new PdfExporter(), templateContent, fileName);
        }

        /// <summary>
        /// 导出HTML模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">数据</param>
        /// <param name="template">模板文件名</param>
        /// <param name="fileName">导出文件名</param>
        /// <returns></returns>
        public static async Task<FileResult> ExportToHtml<T>(T type, string template = null, string fileName = null) where T : class, new()
        {
            var templateContent = await GetTemplateContent(template);
            return await ExportByTemplate(type, new HtmlExporter(), templateContent, fileName);
        }

        /// <summary>
        /// 导出excel模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type">数据</param>
        /// <param name="template">模板文件名</param>
        /// <param name="fileName">导出文件名</param>
        /// <param name="exporter">导出程序</param>
        /// <returns></returns>
        public static async Task<FileResult> ExportByTemplate<T>(T type, IExportFileByTemplate exporter, string template, string fileName = null) where T : class, new()
        {
            var content = await exporter.ExportBytesByTemplate(type, template);
            var fix = exporter switch
            {
                ExcelExporter => "xlsx",
                PdfExporter => "pdf",
                HtmlExporter => "html",
                WordExporter => "docx",
                _ => "xlsx",
            };
            return GetFileResult(content, fileName ?? $"{DateTime.Now:yyyyMMddHHmmss}.{fix}");
        }

        /// <summary>
        /// 导出word模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataItems">数据</param>
        /// <param name="template">模板文件名</param>
        /// <param name="fileName">导出文件名</param>
        /// <returns></returns>
        public static async Task<FileResult> ExportListToWord<T>(ICollection<T> dataItems, string template = null, string fileName = null) where T : class, new()
        {
            var exporter = new WordExporter();
            var templateContent = await GetTemplateContent(template);
            var content = await exporter.ExportBytesByTemplate(dataItems, templateContent);
            return GetFileResult(content, fileName ?? $"{DateTime.Now:yyyyMMddHHmmss}.docx");
        }

        /// <summary>
        /// 导出PDF模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataItems">数据</param>
        /// <param name="template">模板文件名</param>
        /// <param name="fileName">导出文件名</param>
        /// <param name="pdfExporterAttribute">PDF导出特性</param>
        /// <returns></returns>
        public static async Task<FileResult> ExportListToPdf<T>(ICollection<T> dataItems, string template = null, string fileName = null, PdfExporterAttribute pdfExporterAttribute = null) where T : class, new()
        {
            if (pdfExporterAttribute == null)
            {
                var type = typeof(T);
                pdfExporterAttribute = type.GetAttribute<PdfExporterAttribute>(true);
                if (pdfExporterAttribute == null)
                {
                    var exporterAttribute = type.GetAttribute<ExporterAttribute>(true) ?? new PdfExporterAttribute();
                    pdfExporterAttribute = new PdfExporterAttribute
                    {
                        FontSize = exporterAttribute.FontSize,
                        HeaderFontSize = exporterAttribute.HeaderFontSize
                    };
                }
            }
            var exporter = new PdfExporter();
            var templateContent = await GetTemplateContent(template);
            var content = await exporter.ExportListBytesByTemplate(dataItems, pdfExporterAttribute, templateContent);
            return GetFileResult(content, fileName ?? $"{DateTime.Now:yyyyMMddHHmmss}.pdf");
        }

        /// <summary>
        /// 获取模板内容
        /// </summary>
        /// <returns></returns>
        private static async Task<string> GetTemplateContent(string template)
        {
            if (string.IsNullOrWhiteSpace(template)) template = DefaultHtmlTemplate;
            return await File.ReadAllTextAsync(template);
        }

        /// <summary>
        /// 默认html模板
        /// </summary>
        private static string _defaultHtmlTemplate;

        /// <summary>
        /// 默认html模板
        /// </summary>
        private static string DefaultHtmlTemplate
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_defaultHtmlTemplate)) return _defaultHtmlTemplate;
                var webhost = Static.HttpContext.Current.RequestServices.GetService<IWebHostEnvironment>();
                _defaultHtmlTemplate = Path.Combine(webhost.WebRootPath, "fileoper", "export.cshtml");
                return _defaultHtmlTemplate;
            }
        }

        /// <summary>
        /// 获取文件类型
        /// </summary>
        /// <returns></returns>
        private static FileResult GetFileResult(byte[] content, string fileName)
        {
            fileName.CheckNotNullOrWhiteSpace("文件名");
            var contentType = Path.GetExtension(fileName).ToLower()[1..];
            contentType = contentType switch
            {
                "xlsx" => XLSXMediaType,
                "pdf" => PDFMediaType,
                "doc" => DOCMediaType,
                "html" => HTMLMediaType,
                _ => XLSXMediaType,
            };
            return new FileContentResult(content, contentType)
            {
                FileDownloadName = fileName
            };
        }

        /// <summary>
        /// excel
        /// </summary>
        private const string XLSXMediaType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        /// <summary>
        /// PDF
        /// </summary>
        private const string PDFMediaType = "application/pdf";

        /// <summary>
        /// word
        /// </summary>
        private const string DOCMediaType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

        /// <summary>
        /// 网页
        /// </summary>
        private const string HTMLMediaType = "text/html";
    }
}