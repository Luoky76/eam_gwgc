using EAM.Material.Services;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class SpStoreController : AreaController
    {
        private readonly SpStoreService _service;

        public SpStoreController(SpStoreService service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> ComboxDataAsync()
        {
            return await _service.ComboxDataAsync();
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> ListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.ListAsync(request), "成功");
        }

        /// <summary>
        /// 保存
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> SaveAsync(SaveRequest<SP_STORE> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.SaveAsync(request);
        }

        /// <summary>
        /// 获取树形数据
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> TreeAsync()
        {
            return await _service.TreeAsync();
        }

        #region 库存预警

        /// <summary>
        /// 获取库存汇总列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> StoreSumListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.StoreSumListAsync(request), "成功");
        }

        /// <summary>
        /// 获取预警列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> LimitListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.LimitListAsync(request), "成功");
        }

        /// <summary>
        /// 获取库存预警列表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> StoreLimitListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.StoreLimitListAsync(request), "成功");
        }

        /// <summary>
        /// 保存预警设置
        /// </summary>
        [HttpPost]
        [JsToken]
        public async Task<AjaxResult> LimitSaveAsync(SaveRequest<SP_LIMIT> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.LimitSaveAsync(request);
        }

        /// <summary>
        /// 设置上下限
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> SetTopLower(string LIMITID, int? TOP, int? LOWER)
        {
            return AjaxResult.Success(await _service.SetTopLower(LIMITID, TOP, LOWER), "成功");
        }

        #endregion

        #region 库存报表

        /// <summary>
        /// 获取报表下拉框数据
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> ReportComboxDataAsync()
        {
            return await _service.ReportComboxDataAsync();
        }

        /// <summary>
        /// 物资查询
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> StoreSearchListAsync(GridRequest request)
        {
            return AjaxResult.Success(await _service.StoreSearchListAsync(request), "成功");
        }

        /// <summary>
        /// 收发存报表
        /// </summary>
        [HttpPost]
        public async Task<AjaxResult> StoreInOutListAsync(DateTime? CREATEDATE, GridRequest request)
        {
            return AjaxResult.Success(await _service.StoreInOutListAsync(CREATEDATE, request), "成功");
        }

        #endregion
    }
}
