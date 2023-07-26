using EAM.Device.interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Device.controller
{
    [GksybAuthorize(true)]
    public class RepInfoController : AreaController
    {
        private readonly IRepInfoService _service;

        public RepInfoController(IRepInfoService service)
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
                faultSrc = comboxData["FaultSrc"],
                shipInfo = comboxData["ShipInfo"],
                frdbLevel = comboxData["FrdbLevel"],
            }, "成功");
        }

        /// <summary>
        /// 获取故障库记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetRepFrdbListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetRepFrdbList(request), "成功");
        }

        /// <summary>
        /// 根据ID获取信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetRepFrdbListDetailAsync(string ID)
        {
            if (ID.IsNullOrEmpty()) return AjaxResult<REP_FRDB>.Error("请传递参数");
            return AjaxResult.Success(await _service.GetRepFrdbListDetail(ID), "成功");
        }

        /// <summary>
        /// 管理故障库记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageRepFrdbAsync(SaveRequest<REP_FRDB> request)
        {
            return await _service.ManageRepFrdb(request);
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

        /// <summary>
        /// 获取故障分类
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetRepTypeListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetRepTypeList(request), "成功");
        }

        /// <summary>
        /// 管理故障分类
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageRepTypeAsync(SaveRequest<REP_TYPE> request)
        {
            return await _service.ManageRepType(request);
        }
    }
}