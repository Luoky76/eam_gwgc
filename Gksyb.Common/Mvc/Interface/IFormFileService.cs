using Gksyb.Common.Mvc.Dtos;

namespace Gksyb.Common.Mvc.Interface
{
    public interface IFormFileService : IService
    {
        /// <summary>
        /// 文件保存
        /// </summary>
        Task<string> SaveAsync(FormFileRequest request);
    }
}