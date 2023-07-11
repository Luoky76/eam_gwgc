using Gksyb.Common;
using Gksyb.Core.Application;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAM.Device.interfaces
{
    public interface IInventoryTaskService : IService
    {
        /// <summary>
        /// 获取下拉
        /// </summary>
        /// <returns></returns>
        public Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData();

        /// <summary>
        /// 获取人员下拉
        /// </summary>
        /// <returns></returns>
        public Task<List<ComboxData>> UserData();

        /// <summary>
        /// 获取设备盘点任务列表
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetDeviceScanList(GridRequest request);

        /// <summary>
        /// 查看设备盘点明细
        /// </summary>
        /// <returns></returns>
        Task<AjaxResult> GetDeviceScanDetail(long? ID);


        /// <summary>
        /// 管理设备盘点任务列表
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManageDeviceScan(SaveRequest<DEVICE_SCAN> request);

        /// <summary>
        /// 生成盘点清单
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> MakeScanList(string sid, string deptid, string typeid);

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> Submit(List<string> sids);

        /// <summary>
        /// 获取获取设备盘点结果
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetDeviceScanResult(GridRequest request);
    }
}