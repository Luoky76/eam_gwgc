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

        #region 劳保用品租借

        Task<GridData> LaborRentList(GridRequest request);

        Task<GridData> GetLaborRentDetList(string rentId);

        Task<AjaxResult> LaborRentGet(string rentId);
        Task<AjaxResult> LaborRentSave(SaveRequest<LABOR_RENT> request, SaveRequest<LABOR_RENT_DET> requestdet);

        #endregion
    }
}
