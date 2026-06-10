using EAM.Material.Services;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class BaseSpHouseController : AreaController
    {
        private readonly BaseSpHouseService _service;

        public BaseSpHouseController(BaseSpHouseService service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public async Task<AjaxResult> ComboxDataAsync()
        {
            return await _service.ComboxDataAsync();
        }

        /// <summary>
        /// 获取仓库下拉数据
        /// </summary>
        public async Task<AjaxResult> HouseComboxDataAsync()
        {
            return AjaxResult.Success(data: await _service.HouseComboxDataAsync());
        }

        /// <summary>
        /// 根据ID获取记录
        /// </summary>
        public async Task<AjaxResult> GetAsync(string houseId)
        {
            return AjaxResult.Success(await _service.GetAsync(houseId));
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<AjaxResult> ListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.ListAsync(request));
        }

        /// <summary>
        /// 新增
        /// </summary>
        [JsToken, GksybAuthorize(BtnNo = GksybAuthorizeAttribute.AddBtn)]
        public async Task<AjaxResult> AddAsync(SP_HOUSE model)
        {
            var request = new SaveRequest<SP_HOUSE> { Added = new List<SP_HOUSE> { model } };
            var result = await _service.SaveAsync(request);
            if (result.IsError) return result;
            result.Data = model;
            return result;
        }

        /// <summary>
        /// 修改
        /// </summary>
        [JsToken, GksybAuthorize(BtnNo = GksybAuthorizeAttribute.UpdateBtn)]
        public async Task<AjaxResult> UpdateAsync(SP_HOUSE model, SP_HOUSE original)
        {
            var request = new SaveRequest<SP_HOUSE> { Updated = new List<SP_HOUSE> { model } };
            if (original != null) request.Original = new List<SP_HOUSE> { original };
            var result = await _service.SaveAsync(request);
            if (result.IsError) return result;
            result.Data = model;
            return result;
        }

        /// <summary>
        /// 删除
        /// </summary>
        [JsToken, GksybAuthorize(BtnNo = GksybAuthorizeAttribute.DeleteBtn)]
        public async Task<AjaxResult> DeleteAsync(List<SP_HOUSE> list)
        {
            var request = new SaveRequest<SP_HOUSE> { Deleted = list };
            return await _service.SaveAsync(request);
        }

        /// <summary>
        /// 保存
        /// </summary>
        [JsToken]
        public async Task<AjaxResult> SaveAsync(SaveRequest<SP_HOUSE> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.SaveAsync(request);
        }

        /// <summary>
        /// 获取树形结构
        /// </summary>
        public async Task<AjaxResult> TreeAsync()
        {
            return await _service.TreeAsync();
        }

        /// <summary>
        /// Excel导入
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> ImportAsync([FileOptions("xlsx,xls", 1)] IFormFile formFile)
        {
            return await _service.ImportAsync(formFile);
        }
    }
}
