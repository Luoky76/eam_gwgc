using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Interfaces
{
    public interface ISpStoreService : IService
    {
        /// <summary>
        /// 获取列表信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GridData> ListAsync(GridRequest request);

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AjaxResult> Save(SaveRequest<SP_STORE> request);

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        /// <returns></returns>
        Task<AjaxResult> ComboxData();

        Task<AjaxResult> TreeAsync();

        /// <summary>
        /// 库存预警
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GridData> LimitListAsync(GridRequest request);
        Task<GridData> StoreSumListAsync(GridRequest request);
        Task<GridData> StoreLimitListAsync(GridRequest request);
        Task<AjaxResult> LimitSave(SaveRequest<SP_LIMIT> request);
        Task<int> SetTopLower(string LIMITID, int? TOP, int? LOWER);

        #region 库存报表
        Task<AjaxResult> ReportComboxData();
        Task<GridData> StoreSearchListAsync(GridRequest request);
        Task<GridData> StoreInOutListAsync(DateTime? CREATEDATE, GridRequest request);
        #endregion
    }
}