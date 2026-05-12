using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System.Collections.Concurrent;

namespace EAM.Device.interfaces
{
    public interface IRepInfoService : IService
    {
        /// <summary>
        /// 获取下拉
        /// </summary>
        /// <returns></returns>
        public Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData();

        /// <summary>
        /// 获取故障库记录
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetRepFrdbList(GridRequest request);

        /// <summary>
        /// 获取单条故障库记录
        /// </summary>
        /// <returns></returns>
        Task<REP_FRDB> GetRepFrdbListDetail(string ID);

        /// <summary>
        /// 管理故障库记录
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManageRepFrdb(SaveRequest<REP_FRDB> request);

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        public Task<int> Submit(List<string> sids);

        /// <summary>
        /// 获取故障分类
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetRepTypeList(GridRequest request);

        /// <summary>
        /// 管理故障分类
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManageRepType(SaveRequest<REP_TYPE> request);
    }
}