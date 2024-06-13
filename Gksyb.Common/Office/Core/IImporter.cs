namespace Gksyb.Common.Office.Core
{
    /// <summary>
    /// 导入
    /// </summary>
    public interface IImporter
    {
        /// <summary>
        /// 生成导入模板
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>二进制字节</returns>
        Task<byte[]> GenerateTemplateBytes<T>() where T : class, new();


        /// <summary>
        /// 导入模型验证数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="labelingFilePath">标注文件路径</param>
        /// <param name="importResultCallback">导入结果回调函数</param>
        /// <returns></returns>
        Task<ImportResult<T>> Import<T>(string filePath, string labelingFilePath = null, Func<ImportResult<T>, ImportResult<T>> importResultCallback = null) where T : class, new();


        /// <summary>
        /// 导入模型验证数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="importResultCallback">导入结果回调函数</param>
        /// <returns></returns>
        Task<ImportResult<T>> Import<T>(string filePath, Func<ImportResult<T>, ImportResult<T>> importResultCallback) where T : class, new();


        /// <summary>
        /// 导入模型验证数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream">文件流</param>
        /// <returns></returns>
        Task<ImportResult<T>> Import<T>(Stream stream) where T : class, new();

        /// <summary>
        /// 导入模型验证数据并返回错误标注Stream
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <param name="labelingFileStream"></param>
        /// <returns></returns>
        Task<ImportResult<T>> Import<T>(Stream stream, Stream labelingFileStream, Func<ImportResult<T>, ImportResult<T>> importResultCallback = null) where T : class, new();
    }
}