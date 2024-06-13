namespace Gksyb.Common.Office.Html
{
    /// <summary>
    /// 根据模板导出列表字符串
    /// </summary>
    public interface IHtmlExport
    {
        /// <summary>
        /// 根据模板导出列表
        /// </summary>
        /// <param name="data">集合数据</param>
        /// <param name="template">Html模板内容</param>
        Task<string> ExportListByTemplate<T>(ICollection<T> data, string template = null) where T : class;

        /// <summary>
        /// 根据模板导出
        /// </summary>
        /// <param name="data">数据</param>
        /// <param name="template">Html模板内容</param>
        Task<string> ExportByTemplate<T>(T data, string template = null) where T : class;
    }
}