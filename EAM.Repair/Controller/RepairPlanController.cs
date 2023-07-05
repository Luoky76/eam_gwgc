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
            var comboxData = await _service.ComboxData();
            return AjaxResult.Success(new
            {
                ShipList = comboxData["ShipList"],
                MaintDept = comboxData["MaintDept"],
                RepairType = comboxData["RepairType"],
                RepairDealType = comboxData["RepairDealType"]
            }, "成功");
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
        public async Task<AjaxResult> ShipList()
        {
            return await _service.ShipList();
        }
    }
}
