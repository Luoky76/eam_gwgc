namespace Gksyb.Common.Office.Core
{
    /// <summary>
    /// 根据模板导出文件
    /// </summary>
    public interface IExportFileByTemplate
    {
        /// <summary>
        /// 根据模板导出
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="template">Html模板内容</param>
        Task<byte[]> ExportBytesByTemplate<T>(T data, string template) where T : class;

        /// <summary>
        /// 根据模板导出
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="template">Html模板内容</param>
        Task<byte[]> ExportBytesByTemplate(object data, string template, Type type);
    }
}