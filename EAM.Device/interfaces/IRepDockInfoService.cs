using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System.Collections.Concurrent;

namespace EAM.Device.interfaces
{
    public interface IRepDockInfoService : IService
    {
        /// <summary>
        /// 获取下拉
        /// </summary>
        /// <returns></returns>
        public Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData();

        /// <summary>
        /// 获取码头记录
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetBaseDockList(GridRequest request);

        /// <summary>
        /// 获取单条码头记录
        /// </summary>
        /// <returns></returns>
        Task<BASE_DOCK> GetBaseDockListDetail(string ID);

        /// <summary>
        /// 管理码头记录
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManageBaseDock(SaveRequest<BASE_DOCK> request);

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        public Task<int> Submit(List<string> sids);

        /// <summary>
        /// 获取维修计划记录
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetRepDockPlanList(GridRequest request);

        /// <summary>
        /// 获取单条维修计划记录
        /// </summary>
        /// <returns></returns>
        Task<REP_DOCK_PLAN> GetRepDockPlanListDetail(string ID);

        /// <summary>
        /// 管理维修计划记录
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManageRepDockPlan(SaveRequest<REP_DOCK_PLAN> request);

        /// <summary>
        /// 提交维修计划
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> SubmitRepDockPlan(List<string> sids);

        /// <summary>
        /// 获取计划明细
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetPlandetList(GridRequest request);

        /// <summary>
        /// 管理计划明细
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManagePlandet(SaveRequest<REP_DOCK_PLAN_ITEM> request);

        /// <summary>
        /// 提交维修实施
        /// </summary>
        /// <returns></returns>
        public Task<int> SubmitRepDockExe(List<string> sids);

        /// <summary>
        /// 管理维修实施结果
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManageRepDockExe(SaveRequest<REP_DOCK_PLAN> request);

        /// <summary>
        /// 获取维修实施记录
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetRepDockExeList(GridRequest request);

        /// <summary>
        /// 提交码头维修确认
        /// </summary>
        /// <returns></returns>
        public Task<int> SubmitRepDockConfirm(List<string> sids);

        /// <summary>
        /// 获取码头维修确认列表
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetRepDockConfirmList(GridRequest request);

        /// <summary>
        /// 获取单条确认记录
        /// </summary>
        /// <returns></returns>
        Task<REP_DOCK_CHECK> GetRepDockConfirmDetail(string ID);

        /// <summary>
        /// 提交码头维修验收
        /// </summary>
        /// <returns></returns>
        public Task<int> SubmitRepDockCheck(List<string> sids);

        /// <summary>
        /// 获取码头维修验收列表
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetRepDockCheckList(GridRequest request);

        /// <summary>
        ///  管理码头维修验收
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManageRepDockCheck(SaveRequest<REP_DOCK_CHECK> request);
    }
}