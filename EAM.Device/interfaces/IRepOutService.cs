using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System.Collections.Concurrent;

namespace EAM.Device.interfaces
{
    public interface IRepOutService : IService
    {
        /// <summary>
        /// 获取下拉
        /// </summary>
        /// <returns></returns>
        public Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData();

        /// <summary>
        /// 提交委外维修确认
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> SubmitRepOutCheck(List<string> sids);

        /// <summary>
        /// 获取委外维修确认列表
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetRepOutCheckList(GridRequest request);

        /// <summary>
        /// 管理委外维修确认
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManageRepOut(SaveRequest<REP_OUT> request);

        /// <summary>
        /// 获取单条停机记录
        /// </summary>
        /// <returns></returns>
        Task<AjaxResult> GetRepOutDetail(string ID);

        /// <summary>
        /// 提交委外维修验收
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> SubmitRepOutAccept(List<string> sids);

        /// <summary>
        /// 获取委外维修验收列表
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetRepOutAcceptList(GridRequest request);

        /// <summary>
        ///  管理委外维修验收
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManageRepOutAccept(SaveRequest<REP_OUT> request);

        /// <summary>
        /// 获取委外维修验收明细列表
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetRepOutAcceptDetail(GridRequest request);

    }
}