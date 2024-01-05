using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System.Collections.Concurrent;

namespace EAM.Device.interfaces
{
    public interface IPmInfoService : IService
    {
        /// <summary>
        /// 获取下拉
        /// </summary>
        /// <returns></returns>
        public Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData();

        /// <summary>
        /// 导入功能
        /// </summary>
        /// <returns></returns>
        public Task<GridData> ImportList(GridRequest request);

        /// <summary>
        /// 获取维保计划记录
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetPmPlanList(GridRequest request);

        /// <summary>
        /// 获取单条维保计划记录
        /// </summary>
        /// <returns></returns>
        Task<PM_PLAN_EXE> GetPmPlanListDetail(string ID);

        /// <summary>
        /// 管理维保计划记录
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManagePmPlan(SaveRequest<PM_PLAN_EXE> request);

        /// <summary>
        /// 提交维保计划
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> SubmitPmPlan(List<string> sids);

        /// <summary>
        /// 反交维保计划
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> UnSubmitPmPlan(string sid);

        /// <summary>
        /// 获取计划明细
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetPlandetList(GridRequest request);

        /// <summary>
        /// 获取计划主表和明细信息
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetExtendPlanList(GridRequest request);

        /// <summary>
        /// 管理计划明细
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManagePlandet(SaveRequest<PM_PLAN_DONEITEM> request);

        /// <summary>
        /// 导入功能
        /// </summary>
        /// <returns></returns>
        public Task<GridData> ImportSpList(GridRequest request);

        /// <summary>
        /// 获取维保人员明细
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetPmPepList(GridRequest request, string exeId, string doneitemId);

        /// <summary>
        /// 获取维保物资明细
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetPmSpList(GridRequest request, string exeId, string doneitemId);

        /// <summary>
        /// 获取作业明细
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetWorkList(GridRequest request);

        /// <summary>
        /// 管理维保人员明细
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManagePmPep(SaveRequest<PM_PLAN_LABOR> request);

        /// <summary>
        /// 管理物资明细
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManagePmSp(SaveRequest<PM_PLAN_SP> request);

        /// <summary>
        /// 管理作业明细
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManageWork(SaveRequest<PM_SPECIAL_WORK> request);

        /// <summary>
        /// 提交维保实施
        /// </summary>
        /// <returns></returns>
        public Task<int> SubmitPmExe(List<string> sids);

        /// <summary>
        /// 反提交维保实施
        /// </summary>
        /// <returns></returns>
        public Task<int> UnSubmitPmExe(string sid);

        /// <summary>
        /// 管理维保实施结果
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManagePmExe(SaveRequest<PM_PLAN_EXE> request);

        /// <summary>
        /// 获取维保实施记录
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetPmExeList(GridRequest request);

        /// <summary>
        /// 获取维保实施查询记录
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetPmExeQryList(GridRequest request);
        /// <summary>
        /// 获取附件
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Task<AjaxResult> GetPmFileList(string Id);

    }
}