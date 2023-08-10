using EAM.Special.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Special.Controller
{
    [GksybAuthorize(MenuNo = "labor")]
    public class LaborController : AreaController
    {
        private readonly ILaborService _service;

        public LaborController(ILaborService laborService) { 
        
        _service= laborService;
        }

        [HttpPost]
        public async Task<AjaxResult> ComboxData()
        {
            return await _service.ComboxData();
        }
        #region 劳保人员清单
        [HttpPost]
        public async Task<GridData> laborUserListAsync(GridRequest request)
        {
            return await _service.laborUserListAsync(request);
        }
        [HttpPost]
        public async Task<AjaxResult> SaveAsync(SaveRequest<LABOR_USER> request)
        {
            return await _service.SaveAsync(request);
        }



        #endregion


    }
}
