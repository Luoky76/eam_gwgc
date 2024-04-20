namespace Gksyb.Common.Office.Core
{
    /// <summary>
    /// 列头过滤
    /// </summary>
    public interface IExporterHeaderFilter : IBaseService
    {
        /// <summary>
        /// 过滤列头（可以在此处理列名、是否隐藏等）
        /// </summary>
        /// <returns></returns>
        ExporterHeaderInfo Filter(ExporterHeaderInfo exporterHeaderInfo);
    }
}