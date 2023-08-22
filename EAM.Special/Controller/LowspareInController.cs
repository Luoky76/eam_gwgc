using DocumentFormat.OpenXml.Office2010.Excel;
using EAM.Special.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Special.Controller
{

    [GksybAuthorize(MenuNo = "LowspareIn")]
    public class LowspareInController : AreaController
    {

        private readonly ILowspareInService _service;
        /// <summary>
        /// 低值品入账
        /// </summary>
        /// <param name="service"></param>
        public LowspareInController(ILowspareInService service)
        {
            _service = service;
        }

        /// <summary>
        /// 通过ID查询记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<SPEC_LOWSPARE_IN>> GetAsync(string sdid)
        {
            if (sdid.IsNullOrEmpty()) return AjaxResult<SPEC_LOWSPARE_IN>.Error("请传递参数");
            return AjaxResult<SPEC_LOWSPARE_IN>.Success(await _service.GetAsync(sdid), "成功");
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<GridData> ListAsync(GridRequest request)
        {
            return await Service.ListAsync(request);
        }

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ComboxData()
        {
            return await _service.ComboxData();
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveAsync(SaveRequest<SPEC_LOWSPARE_IN> request)
        {
            return await _service.SaveAsync(request);
        }

        /// <summary>
        ///低值品
        /// </summary>
        private ILowspareInService Service
        {
            get
            {
                return _service;
            }
        }

    }
}
