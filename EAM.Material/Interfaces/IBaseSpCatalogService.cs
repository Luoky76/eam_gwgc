using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Interfaces
{
    public interface IBaseSpCatalogService : IService
    {
        /// <summary>
        /// 下拉框数据
        /// </summary>
        /// <returns></returns>
        Task<AjaxResult> ComboxData();
        /// <summary>
        /// 获取树形结构
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> TreeAsync();


        /// <summary>
        /// 根据ID获取记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<BASE_SPCATALOG> GetAsync(object id);

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<GridData> ListAsync(GridRequest request);

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<AjaxResult> SaveAsync(SaveRequest<BASE_SPCATALOG> request);

    }
}