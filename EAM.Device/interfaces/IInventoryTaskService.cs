using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System.Collections.Concurrent;

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
        /// 获取设备盘点任务明细
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetDeviceScanDetails(GridRequest request);

        /// <summary>
        /// 管理设备盘点任务明细列表
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManageScanDetail(SaveRequest<DEVICE_SCAN_DET> request);

        /// <summary>
        /// 获取获取设备盘点结果
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetDeviceScanResult(GridRequest request);

        /// <summary>
        /// 提交盘点明细结果
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> SubmitScanDet(string sid);

        /// <summary>
        /// 获取盈亏记录列表
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetUpDownList(GridRequest request);

        /// <summary>
        /// 管理盈亏记录
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManageUpDown(SaveRequest<DEVICE_SCAN_RESULT> request);
    }
}