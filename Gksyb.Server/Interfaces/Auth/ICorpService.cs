using Gksyb.Core.Application;
using Gksyb.Model.Core;
using Gksyb.Model.UI;

namespace Gksyb.Server.Interfaces.Auth
{
    /// <summary>
    /// 组织管理
    /// </summary>
    public interface ICorpService : IService<CF_CORP>, Core.Interfaces.Auth.ICorpService
    {
        /// <summary>
        /// 获取树形结构
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> TreeAsync();

        /// <summary>
        /// 获取树形下拉
        /// </summary>
        /// <returns></returns>
        public Task<List<ComboxData>> CorpData();
    }
}