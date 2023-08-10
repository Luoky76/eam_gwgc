using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Interfaces
{
    public interface IBaseSpCatalogService : IService
    {
        /// <summary>
        /// 获取列表信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GridData> ListAsync(GridRequest request);

        /// <summary>
        /// 获取单行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<AjaxResult> GetAsync(string id);

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        /// <returns></returns>
        Task<AjaxResult> ComboxData();
    }
}