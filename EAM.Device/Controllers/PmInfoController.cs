using EAM.Device.services;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Device.controller
{
    [GksybAuthorize(true)]
    public class PmInfoController : AreaController
    {
        private readonly PmInfoService _service;

        public PmInfoController(PmInfoService service)
        {
            _service = service;
        }

        /// <summary>
        /// 下拉
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ComboxData()
        {
            var comboxData = await _service.ComboxData();
            return AjaxResult.Success(new
            {
                pmType = comboxData["PmType"],
                bySource = comboxData["BySource"],
                maintDept = comboxData["MaintDept"],
                workState = comboxData["WorkState"],
                maintCycle = comboxData["MaintCycle"],
                storeSou = comboxData["BCCode"],
                deviceInfo = comboxData["DeviceInfo"],
            }, "成功");
        }

        /// <summary>
        /// 同时保存维保计划、维保实施的所有主子表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveAllAsync
            (SaveRequest<PM_PLAN_EXE> request1, SaveRequest<PM_PLAN_DONEITEM> request2, SaveRequest<PM_PLAN_SP> request3, SaveRequest<PM_PLAN_LABOR> request4, SaveRequest<PM_SPECIAL_WORK> request5)
        {
            request1.Added ??= new List<PM_PLAN_EXE>();
            request1.Updated ??= new List<PM_PLAN_EXE>();
            request1.Deleted ??= new List<PM_PLAN_EXE>();
            if (request1.Added.Count + request1.Updated.Count != 1)
            {
                return AjaxResult.Error("主表修改记录有且只能有一条");
            }
            if (request1.Deleted.Any())
            {
                return AjaxResult.Error("同时保存方法不能删除主表");
            }
            return await _service.SaveAllAsync(request1, request2, request3, request4, request5);
        }

        /// <summary>
        /// 导入功能
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ImportListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.ImportList(request), "成功");
        }

        /// <summary>
        /// 获取维保计划记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetPmPlanListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetPmPlanList(request), "成功");
        }

        /// <summary>
        /// 根据维保计划ID获取信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetPmPlanListDetailAsync(string ID)
        {
            if (ID.IsNullOrEmpty()) return AjaxResult<PM_PLAN_EXE>.Error("请传递参数");
            return AjaxResult.Success(await _service.GetPmPlanListDetail(ID), "成功");
        }

        /// <summary>
        /// 管理维保计划记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManagePmPlanAsync(SaveRequest<PM_PLAN_EXE> request)
        {
            return await _service.ManagePmPlan(request);
        }

        /// <summary>
        /// 提交维保计划
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitPmPlanAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.SubmitPmPlan(sids), "成功");
        }

        /// <summary>
        /// 反提交维保计划
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> UnSubmitPmPlanAsync(string sid)
        {
            return AjaxResult.Success(await _service.UnSubmitPmPlan(sid), "成功");
        }

        /// <summary>
        /// 获取计划明细
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetPlandetListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetPlandetList(request), "成功");
        }

        /// <summary>
        /// 获取计划主表和明细信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetExtendPlanListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetExtendPlanList(request), "成功");
        }

        /// <summary>
        /// 获取计划明细
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetPlandetFileAsync(string doneitemId)
        {
            return AjaxResult.Success(await _service.GetPmFileList(doneitemId), "成功");
        }
        /// <summary>
        /// 管理计划明细
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManagePlandetAsync(SaveRequest<PM_PLAN_DONEITEM> request)
        {
            return await _service.ManagePlandet(request);
        }

        /// <summary>
        /// 获取维保人员明细
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetPmPepListAsync(GridRequest request, string exeId, string doneitemId)
        {
            return AjaxResult.Success(await _service.GetPmPepList(request, exeId, doneitemId), "成功");
        }

        /// <summary>
        /// 获取维保物资明细
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetPmSpListAsync(GridRequest request, string exeId, string doneitemId)
        {
            return AjaxResult.Success(await _service.GetPmSpList(request, exeId, doneitemId), "成功");
        }

        /// <summary>
        /// 获取作业明细
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetWorkListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetWorkList(request), "成功");
        }

        /// <summary>
        /// 管理作业明细
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageWorkAsync(SaveRequest<PM_SPECIAL_WORK> request)
        {
            return await _service.ManageWork(request);
        }

        /// <summary>
        /// 导入功能
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ImportSpListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.ImportSpList(request), "成功");
        }

        /// <summary>
        /// 管理维保人员明细
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManagePmPepAsync(SaveRequest<PM_PLAN_LABOR> request)
        {
            return await _service.ManagePmPep(request);
        }

        /// <summary>
        /// 管理维保物资明细
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManagePmSpAsync(SaveRequest<PM_PLAN_SP> request)
        {
            return await _service.ManagePmSp(request);
        }

        /// <summary>
        /// 提交维保实施
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitPmExeAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.SubmitPmExe(sids), "成功");
        }

        /// <summary>
        /// 反提交维保实施
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> UnSubmitPmExeAsync(string sid)
        {
            return AjaxResult.Success(await _service.UnSubmitPmExe(sid), "成功");
        }

        /// <summary>
        /// 获取维保实施记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetPmExeListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetPmExeList(request), "成功");
        }

        /// <summary>
        /// 获取维保查询记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetPmExeQryListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetPmExeQryList(request), "成功");
        }
    }
}