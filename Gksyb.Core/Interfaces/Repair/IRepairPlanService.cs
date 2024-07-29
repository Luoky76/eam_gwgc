using Gksyb.Common;
using Gksyb.Core.Application;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace Gksyb.Core.Interfaces.Repair
{
    public interface IRepairPlanService : IService
    {
        Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData();

        Task<AjaxResult> ShipList();

        Task<GridData> GetDeviceAsync(GridRequest request);

        Task<GridData> ExeListAsync(GridRequest request);

        Task<AjaxResult> GetExeDetailAsync(string ID);

        Task<GridData> ExeItemListAsync(GridRequest request);

        Task<AjaxResult> SaveExe(SaveRequest<REP_PLAN_EXE> request, SaveRequest<REP_PLAN_EXE_ITEM> requestdet);

        Task<AjaxResult> SaveExeItem(SaveRequest<REP_PLAN_EXE_ITEM> requestdet);

        Task<AjaxResult> ApprovalCompletedAsync(string sid, bool isPass);

        Task<AjaxResult> SubmitReportAsync(string exeId);

        Task<AjaxResult> SubmitAuditAsync(string exeId);

        Task<AjaxResult> SubmitExeAsync(string exeId);

        Task<AjaxResult> SubmitCheckAsync(string exeId);

        Task<AjaxResult> RevokeReportAsync(string exeId);

        Task<AjaxResult> RevokeAuditAsync(string exeId);

        Task<AjaxResult> RevokeExeAsync(string exeId);

        Task<AjaxResult> RevokeCheckAsync(string exeId);
    }
}
