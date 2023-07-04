using EAM.Repair.dto;
using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Repair.interfaces
{
    public interface IRepairPlanService : IService
    {
        Task<AjaxResult> ComboxData();
        Task<GridData> ListAsync(GridRequest request);

        Task<AjaxResult> Save(SaveRequest<REP_PLAN> request);
    }
}
