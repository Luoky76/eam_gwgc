using EAM.Device.interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Device.controller
{
    [GksybAuthorize(true)]
    public class RepDockInfoController : AreaController
    {
        private readonly IRepDockInfoService _service;

        public RepDockInfoController(IRepDockInfoService service)
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
                dockInfo = comboxData["DockInfo"],
            }, "成功");
        }

        #region 码头基础信息
        /// <summary>
        /// 获取码头记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetBaseDockListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetBaseDockList(request), "成功");
        }

        /// <summary>
        /// 根据ID获取信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetBaseDockListDetailAsync(string ID)
        {
            if (ID.IsNullOrEmpty()) return AjaxResult<BASE_DOCK>.Error("请传递参数");
            return AjaxResult.Success(await _service.GetBaseDockListDetail(ID), "成功");
        }

        /// <summary>
        /// 管理码头记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageBaseDockAsync(SaveRequest<BASE_DOCK> request)
        {
            return await _service.ManageBaseDock(request);
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.Submit(sids), "成功");
        }

        #endregion

        /// <summary>
        /// 获取维修计划记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetRepDockPlanListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetRepDockPlanList(request), "成功");
        }

        /// <summary>
        /// 根据维修计划ID获取信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetRepDockPlanListDetailAsync(string ID)
        {
            if (ID.IsNullOrEmpty()) return AjaxResult<REP_DOCK_PLAN>.Error("请传递参数");
            return AjaxResult.Success(await _service.GetRepDockPlanListDetail(ID), "成功");
        }

        /// <summary>
        /// 管理维修计划记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageRepDockPlanAsync(SaveRequest<REP_DOCK_PLAN> request)
        {
            return await _service.ManageRepDockPlan(request);
        }

        /// <summary>
        /// 提交维修计划
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitRepDockPlanAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.SubmitRepDockPlan(sids), "成功");
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
        /// 管理计划明细
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManagePlandetAsync(SaveRequest<REP_DOCK_PLAN_ITEM> request)
        {
            return await _service.ManagePlandet(request);
        }

        /// <summary>
        /// 提交维修实施
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitRepDockExeAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.SubmitRepDockExe(sids), "成功");
        }

        /// <summary>
        /// 管理维修实施结果
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageRepDockExeAsync(SaveRequest<REP_DOCK_PLAN> request)
        {
            return await _service.ManageRepDockExe(request);
        }

        /// <summary>
        /// 获取维修实施记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetRepDockExeListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetRepDockExeList(request), "成功");
        }

        /// <summary>
        /// 提交码头维修确认
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitRepDockConfirmAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.SubmitRepDockConfirm(sids), "成功");
        }

        /// <summary>
        /// 获取码头维修确认列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetRepDockConfirmListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetRepDockConfirmList(request), "成功");
        }

        /// <summary>
        /// 管理码头确认维修
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageRepDockConfirmAsync(SaveRequest<REP_DOCK_CHECK> request)
        {
            return await _service.ManageRepDockConfirm(request);
        }

        /// <summary>
        /// 根据码头维修ID获取信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetRepDockConfirmDetailAsync(string ID)
        {
            if (ID==null) return AjaxResult.Error("参数错误");
            return AjaxResult.Success(await _service.GetRepDockConfirmDetail(ID), "成功");
        }

        /// <summary>
        /// 提交码头维修验收
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitRepDockCheckAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.SubmitRepDockCheck(sids), "成功");
        }

        /// <summary>
        ///  获取码头维修验收列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetRepDockCheckListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetRepDockCheckList(request), "成功");
        }

        /// <summary>
        /// 管理码头验收维修
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageRepDockCheckAsync(SaveRequest<REP_DOCK_CHECK> request)
        {
            return await _service.ManageRepDockCheck(request);
        }
    }
}