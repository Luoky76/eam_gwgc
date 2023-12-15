using Gksyb.Core.Interfaces.Repair;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Repair.Controller
{
    [GksybAuthorize(true)]
    public class RepairPlanController : AreaController
    {
        private readonly IRepairPlanService _service;
        public RepairPlanController(IRepairPlanService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<AjaxResult> ComboxData()
        {
            var comboxData = await _service.ComboxData();
            return AjaxResult.Success(new
            {
                ShipList = comboxData["ShipList"],
                MaintDept = comboxData["MaintDept"],
                RepairType = comboxData["RepairType"],
                RepairDealType = comboxData["RepairDealType"],
                RepitemType = comboxData["RepitemType"],
                Auditing = comboxData["Auditing"],
                User = comboxData["User"],
                Corp = comboxData["Corp"],
                PlanState = comboxData["PlanState"],
            }, "成功");
        }

        [HttpPost]
        public async Task<AjaxResult> GetDeviceAsync(GridRequest request)
        {
            var result = await _service.GetDeviceAsync(request);
            return AjaxResult.Success(result);
        }


        [HttpPost]
        public async Task<AjaxResult> SaveItem(SaveRequest<REP_PLAN_ITEM> request)
        {
            return await _service.SaveItem(request);
        }

        [HttpPost]
        public async Task<AjaxResult> ShipList()
        {
            return await _service.ShipList();
        }

        [HttpPost]
        public async Task<AjaxResult> ExeListAsync(GridRequest request)
        {
            var result = await _service.ExeListAsync(request);
            return AjaxResult.Success(result);
        }

        [HttpPost]
        public async Task<AjaxResult> GetExeDetailAsync(string ID)
        {
            return await _service.GetExeDetailAsync(ID);
        }

        [HttpPost]
        public async Task<AjaxResult> ExeItemListAsync(GridRequest request)
        {
            var result = await _service.ExeItemListAsync(request);
            return AjaxResult.Success(result);
        }

        [HttpPost]
        public async Task<AjaxResult> SaveExe(SaveRequest<REP_PLAN_EXE> request, SaveRequest<REP_PLAN_EXE_ITEM> requestdet)
        {
            return await _service.SaveExe(request, requestdet);
        }

        [HttpPost]
        public async Task<AjaxResult> SaveExeItem(SaveRequest<REP_PLAN_EXE_ITEM> requestdet)
        {
            return await _service.SaveExeItem(requestdet);
        }

    }
}
