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
                RepairDealType = comboxData["RepairDealType"],
                RepitemType = comboxData["RepitemType"]
            }, "成功");
        }
        public async Task<AjaxResult> ListAsync(GridRequest request)
        {
            var result = await _service.ListAsync(request);
            return AjaxResult.Success(result);
        }

        public async Task<AjaxResult> ItemListAsync(GridRequest request)
        {
            var result = await _service.ItemListAsync(request);
            return AjaxResult.Success(result);
        }

        public async Task<AjaxResult> GetDeviceAsync(GridRequest request)
        {
            var result = await _service.GetDeviceAsync(request);
            return AjaxResult.Success(result);
        }

        public async Task<AjaxResult> GetDetailAsync(string ID)
        {
            return await _service.GetDetailAsync(ID);
        }

        public async Task<AjaxResult> Save(SaveRequest<REP_PLAN> request)
        {
            return await _service.Save(request);
        }

        public async Task<AjaxResult> SaveItem(SaveRequest<REP_PLAN_ITEM> request)
        {
            return await _service.SaveItem(request);
        }
        public async Task<AjaxResult> ShipList()
        {
            return await _service.ShipList();
        }


        public async Task<AjaxResult> ExeListAsync(GridRequest request)
        {
            var result = await _service.ExeListAsync(request);
            return AjaxResult.Success(result);
        }

        public async Task<AjaxResult> GetExeDetailAsync(string ID)
        {
            return await _service.GetExeDetailAsync(ID);
        }

        public async Task<AjaxResult> ExeItemListAsync(GridRequest request)
        {
            var result = await _service.ExeItemListAsync(request);
            return AjaxResult.Success(result);
        }

        public async Task<AjaxResult> SaveExe(SaveRequest<REP_PLAN_EXE> request, SaveRequest<REP_PLAN_EXE_ITEM> requestdet)
        {
            return await _service.SaveExe(request, requestdet);
        }

        public async Task<AjaxResult> SaveCheck(SaveRequest<REP_PLAN_EXE> request, SaveRequest<REP_PLAN_EXE_ITEM> requestdet)
        {
            return await _service.SaveCheck(request, requestdet);
        }
    }
}
