using EAM.Special.Services;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Special.Controller
{
    [GksybAuthorize(true)]
    public class EnergyReportController : AreaController
    {
        private readonly EnergyReportService _service;

        public EnergyReportController(EnergyReportService service)
        {
            _service = service;
        }

        public async Task<AjaxResult> ComboxDataAsync()
        {
            return await Service.ComboxDataAsync();
        }

        /// <summary>
        /// 获取单行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetAsync(string id)
        {
            if (id == null) return AjaxResult.Error("请传递参数");
            return AjaxResult.Success(await _service.GetAsync(id), "成功");
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> SaveAsync(SaveRequest<REPORT_ENERGY> request)
        {
            return await Service.SaveAsync(request);
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GridListAsync(GridRequest request)
        {
            return AjaxResult.Success(await Service.GridListAsync(request), "成功");
        }

        private EnergyReportService Service
        {
            get
            {
                _service.SetUser(CurrentUser);
                return _service;
            }
        }
    }
}
