using Gksyb.Common;
using EAM.Special.Services;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Special.Controller
{
    [GksybAuthorize(MenuNo = "DrugCollect")]
    public class DrugCollectDetController : AreaController
    {
        private readonly DrugCollectDetService _service;

        /// <summary>
        /// 药品采购明细
        /// </summary>
        /// <param name="service"></param>
        public DrugCollectDetController(DrugCollectDetService service)
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
        /// 获取导入列表
        /// 包含尚未采购的药品SP_ID及总计所需数量
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ImportListAsync(GridRequest request)
        {
            return await _service.ImportListAsync(request);
        }

        /// <summary>
        /// 获取单行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetAsync(string id)
        {
            if (id.IsNullOrEmpty()) return AjaxResult.Error("请传递参数");
            return AjaxResult.Success(await _service.GetAsync(id), "成功");
        }

        /// <summary>
        /// 根据COLLECT_ID获取列表
        /// </summary>
        /// <param name="collectId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<GridData> GetCertainCollectIdAsync(string collectId)
        {
            return await _service.GetCertainCollectIdAsync(collectId);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveAsync(SaveRequest<DRUG_COLLECT_DET> request)
        {
            return await _service.SaveAsync(request);
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
    }
}
