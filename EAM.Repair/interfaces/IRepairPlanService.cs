using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System.Collections.Concurrent;

namespace EAM.Repair.interfaces
{
    public interface IRepairPlanService : IService
    {
        Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData();
        Task<GridData> ListAsync(GridRequest request);

        Task<AjaxResult> GetDetailAsync(string ID);

        Task<AjaxResult> Save(SaveRequest<REP_PLAN> request);

        Task<AjaxResult> SaveItem(SaveRequest<REP_PLAN_ITEM> request);

        Task<AjaxResult> ShipList();

        Task<GridData> ItemListAsync(GridRequest request);

        Task<GridData> GetDeviceAsync(GridRequest request);

        Task<GridData> ExeListAsync(GridRequest request);

        Task<AjaxResult> GetExeDetailAsync(string ID);

        Task<GridData> ExeItemListAsync(GridRequest request);

        Task<AjaxResult> SaveExe(SaveRequest<REP_PLAN_EXE> request, SaveRequest<REP_PLAN_EXE_ITEM> requestdet);

        Task<AjaxResult> SaveExeItem(SaveRequest<REP_PLAN_EXE_ITEM> requestdet);

    }
}
