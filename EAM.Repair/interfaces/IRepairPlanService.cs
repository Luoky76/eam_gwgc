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

        Task<AjaxResult> Save(SaveRequest<REP_PLAN> request);
        Task<AjaxResult> ShipList();
    }
}
