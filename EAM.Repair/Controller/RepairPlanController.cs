using EAM.Repair.dto;
using EAM.Repair.interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Repair.Controller
{
    [GksybAuthorize("RepairPlan")]
    public class RepairPlanController : AreaController
    {
        private readonly IRepairPlanService _service;
        public RepairPlanController(IRepairPlanService service)
        {
            _service = service;
        }
        public async Task<AjaxResult> ComboxData()
        {
            return await _service.ComboxData();
        }
        public async Task<AjaxResult> ListAsync(GridRequest request)
        {
            var result = await _service.ListAsync(request);
            return AjaxResult.Success(result);
        }
        public async Task<AjaxResult> Save(SaveRequest<REP_PLAN> request)
        {
            return await _service.Save(request);
        }
    }
}
