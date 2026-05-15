using EAM.Special.Services;
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

        private readonly LowspareInService _service;
        /// <summary>
        /// 低值品入账
        /// </summary>
        /// <param name="service"></param>
        public LowspareInController(LowspareInService service)
        {
            _service = service;
        }

        /// <summary>
        /// 通过ID查询记录
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetAsync(string inId)
        {
            if (inId.IsNullOrEmpty()) return AjaxResult.Error("请传递参数");
            return AjaxResult.Success(await _service.GetAsync(inId), "成功");
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
        public async Task<AjaxResult> ComboxDataAsync()
        {
            return await _service.ComboxDataAsync();
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
        /// 提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SubmitAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.SubmitAsync(sids), "成功");
        }

        /// <summary>
        ///低值品
        /// </summary>
        private LowspareInService Service
        {
            get
            {
                return _service;
            }
        }

    }
}
