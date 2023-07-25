using EAM.Material.Interfaces;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class BaseSpHouseController : AreaController
    {
        private readonly IBaseSpHouseService _service;

        public BaseSpHouseController(IBaseSpHouseService service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<GridData>> ListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.ListAsync(request), "成功");
        }

        /// <summary>
        /// 获取单行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<AjaxResult> GetAsync(string id)
        {
            return await _service.GetAsync(id);
        }

        /// <summary>
		/// 获取下拉框数据
		/// </summary>
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
        [JsToken]
        public async Task<AjaxResult> Save(SaveRequest<SP_HOUSE> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.Save(request);
        }


        [HttpPost]
        public async Task<AjaxResult> TreeAsync()
        {
            return await _service.TreeAsync();
        }

        [HttpPost]
        public async Task<AjaxResult> SubmitAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.Submit(sids), "成功");
        }
    }
}
