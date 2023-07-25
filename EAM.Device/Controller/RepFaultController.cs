using EAM.Device.interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Device.controller
{
    [GksybAuthorize(true)]
    public class RepFaultController : AreaController
    {
        private readonly IRepFaultService _service;

        public RepFaultController(IRepFaultService service)
        {
            _service = service;
        }

        /// <summary>
        /// 下拉
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ComboxData()
        {
            var comboxData = await _service.ComboxData();
            return AjaxResult.Success(new
            {
                disposeType = comboxData["DisposeType"],
                deviceStatus = comboxData["DeviceStatus"],
                repType = comboxData["RepType"],
                faultSrc = comboxData["FaultSrc"],
                faultStatus = comboxData["FaultStatus"],
                maintDept = comboxData["MaintDept"],
                shipInfo = comboxData["ShipInfo"],
                deviceInfo = comboxData["DeviceInfo"],
            }, "成功");
        }

        /// <summary>
        /// 获取故障处理记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetFaultExeListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetFaultExeList(request), "成功");
        }

        /// <summary>
        /// 根据ID获取信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetFaultExeListDetailAsync(string ID)
        {
            if (ID.IsNullOrEmpty()) return AjaxResult<REP_FAULT>.Error("请传递参数");
            return AjaxResult.Success(await _service.GetFaultExeListDetail(ID), "成功");
        }

        /// <summary>
        /// 管理故障处理记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageFaultExeAsync(SaveRequest<REP_FAULT> request)
        {
            return await _service.ManageFaultExe(request);
        }

        /// <summary>
        /// 提交故障处理
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitFaultExeAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.SubmitFaultExe(sids), "成功");
        }

        /// <summary>
        /// 获取人员明细
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetFaultPepListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetFaultPepList(request), "成功");
        }

        /// <summary>
        /// 获取物资明细
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetFaultSpListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetFaultSpList(request), "成功");
        }

        /// <summary>
        /// 管理人员明细
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageFaultPepAsync(SaveRequest<REP_FAULT_LABOR> request)
        {
            return await _service.ManageFaultPep(request);
        }

        /// <summary>
        /// 管理物资明细
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageFaultSpAsync(SaveRequest<REP_FAULT_SP> request)
        {
            return await _service.ManageFaultSp(request);
        }

        /// <summary>
        /// 提交验收
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitFaultCheckAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.SubmitFaultCheck(sids), "成功");
        }

        /// <summary>
        /// 驳回验收
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitFaultUnCheckAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.SubmitFaultUnCheck(sids), "成功");
        }

        /// <summary>
        /// 管理验收结果
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageFaultCheckAsync(SaveRequest<REP_FAULT> request)
        {
            return await _service.ManageFaultCheck(request);
        }

        /// <summary>
        /// 获取验收记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetFaultCheckListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetFaultCheckList(request), "成功");
        }

        /// <summary>
        /// 获取验收查询记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetFaultCheckQryListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetFaultCheckQryList(request), "成功");
        }
    }
}