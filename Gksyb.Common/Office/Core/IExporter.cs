using System.Data;

namespace Gksyb.Common.Office.Core
{
    /// <summary>
    /// 导出
    /// </summary>
    public interface IExporter
    {
        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="dataItems">数据</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        Task<byte[]> ExportAsByteArray(DataTable dataItems, Type type);

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="dataItems">数据</param>
        /// <returns>文件二进制数组</returns>
        Task<byte[]> ExportAsByteArray<T>(ICollection<T> dataItems) where T : class, new();

        /// <summary>
        /// 导出
        /// </summary>
        /// <param name="dataItems">数据</param>
        /// <returns>文件二进制数组</returns>
        Task<byte[]> ExportAsByteArray<T>(DataTable dataItems) where T : class, new();

        /// <summary>
        /// 导出表头
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>文件二进制数组</returns>
        Task<byte[]> ExportHeaderAsByteArray<T>(T type) where T : class, new();
    }
}