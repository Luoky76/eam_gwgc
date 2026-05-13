using EAM.Special.Services;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Special.Controller
{
    [GksybAuthorize(MenuNo = "DrugCollect")]
    public class DrugCollectController : AreaController
    {
        private readonly DrugCollectService _service;

        /// <summary>
        /// 药品采购登记主表
        /// </summary>
        /// <param name="service"></param>
        public DrugCollectController(DrugCollectService service)
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
        public async Task<AjaxResult<DRUG_COLLECT>> GetAsync(string id)
        {
            if (id.IsNullOrEmpty()) return AjaxResult<DRUG_COLLECT>.Error("请传递参数");
            return AjaxResult<DRUG_COLLECT>.Success(await _service.GetAsync(id), "成功");
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveAsync(SaveRequest<DRUG_COLLECT> request)
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
            return await _service.SubmitAsync(sids);
        }

        /// <summary>
        /// 撤销提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> RevokeAsync(List<string> sids)
        {
            return await _service.RevokeAsync(sids);
        }

        /// <summary>
        /// 同时保存采购主表以及与其关联的采购明细子表、采购需求子表
        /// </summary>
        /// <param name="request1"></param>
        /// <param name="request2"></param>
        /// <param name="request3"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveAllAsync
            (SaveRequest<DRUG_COLLECT> request1, SaveRequest<DRUG_COLLECT_DET> request2, SaveRequest<DRUG_COLLECT_REQUEST> request3)
        {
            return await _service.SaveAllAsync(request1, request2, request3);
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
