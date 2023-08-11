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
        public async Task<GridData> laborrequestdetListAsync(GridRequest request)
        {
            return await _service.laborrequestdetListAsync(request);
        }
        [HttpPost]
        public async Task<GridData> laborrequestListListAsync(GridRequest request)
        {
            return await _service.laborrequestListListAsync(request);
        }

        [HttpPost]
        public async Task<AjaxResult> laborUserSaveAsync(SaveRequest<LABOR_USER> request)
        {
            return await _service.SaveAsync(request);
        }
        #endregion
        #region 劳保需求申请
        [HttpPost]
        public async Task<GridData> laborrequestListAsync(GridRequest request)
        {
            return await _service.laborrequestListAsync(request);
        }



        [HttpPost]
        public async Task<AjaxResult> laborrequestSaveAsync(SaveRequest<LABOR_REQUEST> request)
        {
            return await _service.SaveAsync(request);
        }

        #endregion

        #region 劳保采购计划
        [HttpPost]
        public async Task<GridData> laborcollectListAsync(GridRequest request)
        {
            return await _service.laborcollectListAsync(request);
        }
        [HttpPost]
        public async Task<AjaxResult> laborcollectSaveAsync(SaveRequest<LABOR_COLLECT> request)
        {
            return await _service.SaveAsync(request);
        }
        #endregion

    }
}
