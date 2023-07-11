using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System.Collections.Concurrent;

namespace EAM.Device.interfaces
{
    public interface IDeviceRunService : IService
    {
        /// <summary>
        /// 获取下拉
        /// </summary>
        /// <returns></returns>
        public Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData();
        /// <summary>
        /// 获取设备卡片基础信息
        /// </summary>
        /// <returns></returns>
        public Task<List<ComboxData>> DeviceData();
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetRun(GridRequest request);

        /// <summary>
        /// 增删改
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> Manage(SaveRequest<RUN_TRANS> request);

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> Submit(string sids);

        /// <summary>
        /// 获取运行状态一览表
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetAllRun(GridRequest request);

    }
}
