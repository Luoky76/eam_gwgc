using EAM.Special.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Special.Controller
{
    [GksybAuthorize(MenuNo = "LaborMaterial")]
    public class LaborMaterialDetController : AreaController
    {
        private readonly ILaborMaterialDetService _service;

        /// <summary>
        /// 船舶常规物料清册
        /// </summary>
        /// <param name="service"></param>
        public LaborMaterialDetController(ILaborMaterialDetService service)
        {
            _service = service;
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<GridData> ListAsync(GridRequest request)
        {
            return await _service.ListAsync(request);
        }

        /// <summary>
        /// 获取单行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<LABOR_MATERIAL_DET>> GetAsync(string id)
        {
            if (id.IsNullOrEmpty()) return AjaxResult<LABOR_MATERIAL_DET>.Error("请传递参数");
            return AjaxResult<LABOR_MATERIAL_DET>.Success(await _service.GetAsync(id), "成功");
        }

        /// <summary>
        /// 生成主键
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost]
        public AjaxResult<string> CreatePrimaryKey()
        {
            return AjaxResult<string>.Success(_service.CreatePrimaryKey(), "成功");
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveAsync(SaveRequest<LABOR_MATERIAL_DET> request)
        {
            return await _service.SaveAsync(request);
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
    }
}