using Gksyb.Model;
using Gksyb.Model.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAM.Material.Interfaces
{
    public interface ISpInBackService : IService
    {
        Task<GridData> ListAsync(GridRequest request);

        Task<AjaxResult> GetAsync(string ID);

        Task<AjaxResult> InListAsync();

        Task<GridData> DetListAsync(GridRequest request);

        Task<AjaxResult> Save(SaveRequest<SP_IN_BACK> request, SaveRequest<SP_INBACK_DET> requestdet);
    }
}
