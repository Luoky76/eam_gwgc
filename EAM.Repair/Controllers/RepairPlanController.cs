using EAM.Repair.services;
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
        private readonly RepairPlanService _service;
        public RepairPlanController(RepairPlanService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<AjaxResult> ComboxDataAsync()
        {
            var comboxData = await _service.ComboxDataAsync();
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

        /// <summary>
        /// 故障报修提交
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> SubmitReportAsync(string exeId)
        {
            return await _service.SubmitReportAsync(exeId);
        }

        /// <summary>
        /// 故障核验提交
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> SubmitAuditAsync(string exeId)
        {
            return await _service.SubmitAuditAsync(exeId);
        }

        /// <summary>
        /// 维修实施提交
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> SubmitExeAsync(string exeId)
        {
            return await _service.SubmitExeAsync(exeId);
        }

        /// <summary>
        /// 维修验收提交
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> SubmitCheckAsync(string exeId)
        {
            return await _service.SubmitCheckAsync(exeId);
        }

        /// <summary>
        /// 故障报修撤销提交
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> RevokeReportAsync(string exeId)
        {
            return await _service.RevokeReportAsync(exeId);
        }

        /// <summary>
        /// 故障核验撤销提交
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> RevokeAuditAsync(string exeId)
        {
            return await _service.RevokeAuditAsync(exeId);
        }

        /// <summary>
        /// 维修实施撤销提交
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> RevokeExeAsync(string exeId)
        {
            return await _service.RevokeExeAsync(exeId);
        }

        /// <summary>
        /// 维修验收撤销提交
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> RevokeCheckAsync(string exeId)
        {
            return await _service.RevokeCheckAsync(exeId);
        }
    }
}
