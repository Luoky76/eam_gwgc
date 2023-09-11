using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System.Collections.Concurrent;
using static EAM.Device.services.RepFaultService;

namespace EAM.Device.interfaces
{
    public interface IRepFaultService : IService
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
        /// 获取故障处理记录
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetFaultExeList(GridRequest request);

        /// <summary>
        /// 获取单条故障处理记录
        /// </summary>
        /// <returns></returns>
        Task<REP_FAULT_IMG> GetFaultExeListDetail(string ID);
        /// <summary>
        /// 管理故障处理记录
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManageFaultExe(SaveRequest<REP_FAULT> request);
        /// <summary>
        /// 提交故障处理
        /// </summary>
        /// <returns></returns>
        public Task<int> SubmitFaultExe(List<string> sids);

        /// <summary>
        /// 获取人员明细
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetFaultPepList(GridRequest request);
        /// <summary>
        /// 获取物资明细
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetFaultSpList(GridRequest request);

        /// <summary>
        /// 管理人员明细
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManageFaultPep(SaveRequest<REP_FAULT_LABOR> request);

        /// <summary>
        /// 管理物资明细
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManageFaultSp(SaveRequest<REP_FAULT_SP> request);

        /// <summary>
        /// 提交验收
        /// </summary>
        /// <returns></returns>
        public Task<int> SubmitFaultCheck(List<string> sids);

        /// <summary>
        /// 驳回验收
        /// </summary>
        /// <returns></returns>
        public Task<int> SubmitFaultUnCheck(List<string> sids);

        /// <summary>
        /// 管理验收结果
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManageFaultCheck(SaveRequest<REP_FAULT> request);

        /// <summary>
        /// 获取验收记录
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetFaultCheckList(GridRequest request);

        /// <summary>
        /// 获取验收查询记录
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetFaultCheckQryList(GridRequest request);
    }
}