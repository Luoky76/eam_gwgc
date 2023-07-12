using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System.Collections.Concurrent;

namespace EAM.Device.interfaces
{
    public interface IDeviceStopService : IService
    {
        /// <summary>
        /// 获取下拉
        /// </summary>
        /// <returns></returns>
        public Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData();

        /// <summary>
        /// 获取停机记录
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetStopList(GridRequest request);

        /// <summary>
        /// 获取单条停机记录
        /// </summary>
        /// <returns></returns>
        Task<AjaxResult> GetStopListDetail(string ID);
        /// <summary>
        /// 管理停机记录
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManageStop(SaveRequest<RUN_STOP> request);
        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> Submit(List<string> sids);

        /// <summary>
        /// 获取停机分类
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetStopTypeList(GridRequest request);
        /// <summary>
        /// 管理停机分类
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManageStopType(SaveRequest<RUN_STOP_TYPE> request);

    }
}