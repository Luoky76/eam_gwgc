using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Interfaces
{
    public interface ISpReceiveService : IService
    {
        Task<GridData> ListAsync(GridRequest request);
        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        /// <returns></returns>
        Task<AjaxResult> ComboxData();

        Task<AjaxResult> GetAsync(string ID);
        Task<AjaxResult> Save(SaveRequest<SP_RECEIVE> request, SaveRequest<SP_RECEIVE_DET> requestdet);

        Task<AjaxResult> SaveDet(SaveRequest<SP_RECEIVE_DET> request);

        Task<AjaxResult> OrderList();
        Task<AjaxResult> SpList(GridRequest request);

        Task<GridData> DetListAsync(GridRequest request);
    }
}
