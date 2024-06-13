using Gksyb.Common.Office.Core;
using System.Text;

namespace Gksyb.Common.Office.Html
{
    /// <summary>
    /// HTML导出
    /// </summary>
    public class HtmlExporter : IHtmlExport, IExportFileByTemplate
    {
        private readonly RazorEngineCore.RazorEngine _razor;

        /// <summary>
        /// 初始化
        /// </summary>
        public HtmlExporter()
        {
            _razor = new RazorEngineCore.RazorEngine();
        }

        /// <inheritdoc/>
        public async Task<string> ExportByTemplate<T>(T data, string htmlTemplate) where T : class
            => await RunCompileTplAsync(new ExportDocumentInfo<T>(data), htmlTemplate);

        /// <inheritdoc/>
        public async Task<string> ExportByTemplate(object data, string template, Type type)
            => await RunCompileTplAsync(new ExportDocumentInfo<object>(data, type), template);

        /// <inheritdoc/>
        public async Task<string> ExportListByTemplate<T>(ICollection<T> data, string htmlTemplate = null) where T : class
            => await RunCompileTplAsync(new ExportDocumentInfoOfListData<T>(data), htmlTemplate);

        /// <inheritdoc/>
        public async Task<byte[]> ExportListBytesByTemplate<T>(ICollection<T> data, string template) where T : class
            => await RunCompileTplOfByteAsync(new ExportDocumentInfoOfListData<T>(data), template);

        /// <inheritdoc/>
        public async Task<byte[]> ExportBytesByTemplate<T>(T data, string template) where T : class
            => await RunCompileTplOfByteAsync(new ExportDocumentInfo<T>(data), template);

        /// <inheritdoc/>
        public async Task<byte[]> ExportBytesByTemplate(object data, string template, Type type)
            => await RunCompileTplOfByteAsync(new ExportDocumentInfo<object>(data, type), template);

        /// <summary>
        ///  编译和运行模板
        /// </summary>
        protected async Task<byte[]> RunCompileTplOfByteAsync(object model, string htmlTemplate)
        {
            var content = await RunCompileTplAsync(model, htmlTemplate);
            return Encoding.UTF8.GetBytes(content);
        }

        /// <summary>
        ///  编译和运行模板
        /// </summary>
        protected async Task<string> RunCompileTplAsync(object model, string htmlTemplate)
        {
            var template = await _razor.CompileAsync(htmlTemplate, builder =>
            {
                builder.AddAssemblyReferenceByName("System.Data.Common");
                builder.AddAssemblyReferenceByName("System.Linq");
                builder.AddAssemblyReferenceByName("System.Collections");
                builder.AddAssemblyReferenceByName("Gksyb.Common");
            });
            return await template.RunAsync(model);
        }
    }
}