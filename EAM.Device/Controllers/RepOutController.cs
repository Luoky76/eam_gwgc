using EAM.Device.services;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Device.controller
{
    [GksybAuthorize(true)]
    public class RepOutController : AreaController
    {
        private readonly RepOutService _service;

        public RepOutController(RepOutService service)
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
                maintDept = comboxData["MaintDept"],
                repSourceType = comboxData["RepSourceType"],
                repOutType = comboxData["RepOutType"],
                //providerData = comboxData["ProviderData"],
            }, "成功");
        }

        /// <summary>
        /// 提交委外维修确认
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitRepOutCheckAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.SubmitRepOutCheck(sids), "成功");
        }

        /// <summary>
        /// 获取委外维修确认列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetRepOutCheckListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetRepOutCheckList(request), "成功");
        }

        /// <summary>
        /// 管理委外确认维修
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageRepOutAsync(SaveRequest<REP_OUT> request)
        {
            return await _service.ManageRepOut(request);
        }

        /// <summary>
        /// 根据委外维修ID获取信息
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetRepOutDetailAsync(string ID)
        {
            if (ID == null) return AjaxResult.Error("参数错误");
            return AjaxResult.Success(await _service.GetRepOutDetail(ID), "成功");
        }

        /// <summary>
        /// 提交委外维修验收
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitRepOutAcceptAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.SubmitRepOutAccept(sids), "成功");
        }

        /// <summary>
        ///  获取委外维修验收列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetRepOutAcceptListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetRepOutAcceptList(request), "成功");
        }

        /// <summary>
        /// 管理委外验收维修
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ManageRepOutAcceptAsync(SaveRequest<REP_OUT> request)
        {
            return await _service.ManageRepOutAccept(request);
        }

        /// <summary>
        ///  获取委外维修验收明细列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetRepOutAcceptDetailAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.GetRepOutAcceptDetail(request), "成功");
        }
    }
}