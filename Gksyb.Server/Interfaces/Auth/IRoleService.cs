using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;

namespace Gksyb.Server.Interfaces.Auth
{
    /// <summary>
    /// 角色管理
    /// </summary>
    public interface IRoleService : Core.Interfaces.Auth.IRoleService
    {
        /// <summary>
        /// 获取树形下拉
        /// </summary>
        /// <returns></returns>
        public Task<List<ComboxData>> CorpData();

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public Task<GridData> ListAsync(GridRequest request);

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> SaveAsync(SaveRequest<CF_ROLE> request);
    }
}