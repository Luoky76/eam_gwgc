using EAM.Special.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Special.Controller
{
    [GksybAuthorize(MenuNo = "DrugCollect")]
    public class DrugCollectRequestController : AreaController
    {
        private readonly IDrugCollectRequestService _service;

        /// <summary>
        /// 药品采购-需求连接子表
        /// </summary>
        /// <param name="service"></param>
        public DrugCollectRequestController(IDrugCollectRequestService service)
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
        public async Task<AjaxResult<DRUG_COLLECT_REQUEST>> GetAsync(string id)
        {
            if (id.IsNullOrEmpty()) return AjaxResult<DRUG_COLLECT_REQUEST>.Error("请传递参数");
            return AjaxResult<DRUG_COLLECT_REQUEST>.Success(await _service.GetAsync(id), "成功");
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
        /// 获取需要药品SP_ID的需求
        /// </summary>
        /// <param name="spId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<DRUG_COLLECT_REQUEST>> GetCertainSpIdAsync(string spId)
        {
            return AjaxResult<DRUG_COLLECT_REQUEST>.Success(await _service.GetCertainSpIdAsync(spId));
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveAsync(SaveRequest<DRUG_COLLECT_REQUEST> request)
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
