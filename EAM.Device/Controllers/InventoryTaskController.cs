using EAM.Device.services;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;


namespace EAM.Device.controller
{
    [GksybAuthorize(true)]
    public class InventoryTaskController : AreaController
    {
        private readonly InventoryTaskService _service;

        public InventoryTaskController(InventoryTaskService service)
        {
            _service = service;
        }

        /// <summary>
        /// 下拉
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ComboxDataAsync()
        {
            var comboxData = await _service.ComboxDataAsync();
            return AjaxResult.Success(new
            {
                ScanStatus = comboxData["ScanStatus"],
                DeviceType = comboxData["DeviceTypeName"],
                DeptData = comboxData["DeptData"],
                Status = comboxData["AssetStatus"],
                BCCode = comboxData["BCCode"],
            }, "成功");
        }

        /// <summary>
        /// 设备盘点任务列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetDeviceScanListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetDeviceScanList(request), "成功");
        }


        /// <summary>
        /// 管理设备盘点任务列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageDeviceScanAsync(SaveRequest<DEVICE_SCAN> request)
        {
            return await _service.ManageDeviceScan(request);
        }

        /// <summary>
        /// 生成盘点清单
        /// </summary>
        /// <param name="sid">盘点ID</param>
        /// <param name="deptid">部门ID</param>
        /// <param name="typeid">类型ID</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> MakeScanList(string sid, string deptid, string typeid)
        {
            if (sid.IsNullOrEmpty() || deptid.IsNullOrEmpty())
            {
                return AjaxResult.Error("请传递参数");
            }
            return AjaxResult.Success(await _service.MakeScanList(sid, deptid, typeid), "成功");
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitAsync(List<string> sids)
        {
            await _service.SubmitAsync(sids);
            return AjaxResult.Success("提交成功");
        }


        /// <summary>
        /// 设备盘点明细列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetDeviceScanDetailsAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetDeviceScanDetails(request), "成功");
        }

        /// <summary>
        /// 管理设备盘点任务明细列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageScanDetailAsync(SaveRequest<DEVICE_SCAN_DET> request)
        {
            return await _service.ManageScanDetail(request);
        }

        /// <summary>
        /// 获取设备盘点结果
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetDeviceScanResultAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetDeviceScanResult(request), "成功");
        }

        /// <summary>
        /// 提交盘点明细结果
        /// </summary>
        /// <param name="sid">盘点ID</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitScanDetAsync(string sid)
        {
            return AjaxResult.Success(await _service.SubmitScanDet(sid), "成功");
        }

        /// <summary>
        /// 设备盈亏记录列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetUpDownListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetUpDownList(request), "成功");
        }

        /// <summary>
        /// 反提交盈亏记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitUpDownAsync(string sid)
        {
            return await _service.SubmitUpDown(sid);
        }

        /// <summary>
        /// 提交盈亏记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> UnSubmitUpDownAsync(string sid)
        {
            return await _service.UnSubmitUpDown(sid);
        }

        /// <summary>
        /// 保存盈亏记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageUpDownAsync(SaveRequest<DEVICE_SCAN_RESULT> request)
        {
            return await _service.ManageUpDown(request);
        }
    }
}