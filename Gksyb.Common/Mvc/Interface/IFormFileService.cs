using Microsoft.AspNetCore.Http;

namespace Gksyb.Common.Mvc.Interface
{
    public interface IFormFileService : IService
    {
        /// <summary>
        /// 文件保存
        /// </summary>
        /// <param name="url">web路径</param>
        /// <param name="path">硬盘路径</param>
        /// <param name="mapPath">映射路径</param>
        /// <param name="formFile">上传文件</param>
        /// <returns></returns>
        Task<string> SaveAsync(string url, string path, string mapPath, IFormFile formFile);
    }
}