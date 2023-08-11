using EAM.Material.Interfaces;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class SpStoreController : AreaController
    {
        private readonly ISpStoreService _service;

        public SpStoreController(ISpStoreService service)
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
        public async Task<AjaxResult> Save(SaveRequest<SP_STORE> request)
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

        #region 库存预警
        /// <summary>
        /// 选择库存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<GridData>> StoreSumListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.StoreSumListAsync(request), "成功");
        }

        [HttpPost]
        public async Task<AjaxResult<GridData>> LimitListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.LimitListAsync(request), "成功");
        }

        [HttpPost]
        public async Task<AjaxResult<GridData>> StoreLimitListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.StoreLimitListAsync(request), "成功");
        }

        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> LimitSave(SaveRequest<SP_LIMIT> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.LimitSave(request);
        }

        [HttpPost]
        public async Task<AjaxResult> SetTopLower(string LIMITID, int? TOP, int? LOWER)
        {
            return AjaxResult.Success(await _service.SetTopLower(LIMITID, TOP, LOWER), "成功");
        }
        #endregion
    }
}
