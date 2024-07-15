using EAM.Special.Interfaces;
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
        private readonly IEnergyReportService _service;

        public EnergyReportController(IEnergyReportService service)
        {
            _service = service;
        }

        public async Task<AjaxResult> ComboxData()
        {
            return await Service.ComboxData();
        }

        /// <summary>
        /// 获取单行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<REPORT_ENERGY>> GetAsync(string id)
        {
            if (id == null) return AjaxResult<REPORT_ENERGY>.Error("请传递参数");
            return AjaxResult<REPORT_ENERGY>.Success(await _service.GetAsync(id), "成功");
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
        public async Task<AjaxResult<GridData>> GridListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await Service.GridListAsync(request), "成功");
        }

        private IEnergyReportService Service
        {
            get
            {
                _service.SetUser(CurrentUser);
                return _service;
            }
        }
    }
}
