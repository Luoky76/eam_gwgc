using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Interfaces
{
    public interface ISpInstoreService : IService
    {
        Task<GridData> ListAsync(GridRequest request);

        Task<GridData> DetListAsync(GridRequest request);

        Task<AjaxResult> HouseList();

        Task<GridData> DetailListAsync(GridRequest request);

        Task<AjaxResult> GetAsync(string ID);

        Task<AjaxResult> Save(SaveRequest<SP_INSTORE> request, SaveRequest<SP_INSTORE_DET> requestdet);
    }
}
