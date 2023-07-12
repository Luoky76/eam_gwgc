using EAM.Device.interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EAM.Device.controller
{
    [GksybAuthorize(true)]
    public class InventoryTaskController : AreaController
    {
        private readonly IInventoryTaskService _service;

        public InventoryTaskController(IInventoryTaskService service)
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
                scanStatus = comboxData["ScanStatus"],
                deviceType = comboxData["DeviceTypeName"],
                deptData = comboxData["DeptData"],
            }, "成功");
        }

        /// <summary>
        /// 下拉人员数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> UserData()
        {
            return AjaxResult.Success(new
            {
                userData = await _service.UserData()
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
        /// 根据ID获取信息 查看设备盘点明细
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetDeviceScanDetailAsync(long? ID)
        {
            if (!ID.HasValue) return AjaxResult.Error("参数错误");
            return await _service.GetDeviceScanDetail(ID);
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
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> MakeScanList(string sid, string deptid, string typeid)
        {
            return await _service.MakeScanList(sid, deptid,typeid);
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitAsync(List<string> sids)
        {
            return await _service.Submit(sids);
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
    }
}