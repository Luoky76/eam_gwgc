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

    }
}
