using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using System.Threading.Tasks;

namespace EAM.Special.Interfaces
{
    public interface ILaborService : IService
    {
        Task<GridData> laborUserListAsync(GridRequest request);

        Task<AjaxResult> ComboxData();

        Task<AjaxResult> SaveAsync(SaveRequest<LABOR_USER> request);


        #region 劳保需求申请
        Task<GridData> laborrequestListAsync(GridRequest request);
        Task<AjaxResult> SaveAsync(SaveRequest<LABOR_REQUEST> request);

        Task<GridData> laborrequestdetListAsync(GridRequest request);

        Task<GridData> laborrequestListListAsync(GridRequest request);

        #endregion

        #region 劳保采购计划
        Task<GridData> laborcollectListAsync(GridRequest request);

        Task<AjaxResult> SaveAsync(SaveRequest<LABOR_COLLECT> request);

        #endregion

        #region 劳保用品退换
        Task<GridData> LaborExchangeListAsync(GridRequest request);

        Task<GridData> GetLaborExchangeAppDetList(string id);

        Task<AjaxResult> LaboExchangeGet(string id);
        Task<AjaxResult> LaborExchangeSave(SaveRequest<LABOR_EXCHANGE> request, SaveRequest<LABOR_EXCHANGE_APPDET> requestdet);
        #endregion


        #region 劳保用品租借

        Task<GridData> LaborRentList(GridRequest request);

        Task<GridData> GetLaborRentDetList(string rentId);

        Task<AjaxResult> LaborRentGet(string rentId);

        Task<GridData> LaborStoreList(GridRequest request);

        Task<AjaxResult> LaborRentSave(SaveRequest<LABOR_RENT> request, SaveRequest<LABOR_RENT_DET> requestdet);

        #endregion
    }
}
