using EAM.Special.Services;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Special.Controller
{
    [GksybAuthorize(MenuNo = "DrugRequest")]
    public class DrugRequestController : AreaController
    {
        private readonly DrugRequestService _service;

        /// <summary>
        /// 药品需求主表
        /// </summary>
        /// <param name="service"></param>
        public DrugRequestController(DrugRequestService service)
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
        public async Task<AjaxResult<DRUG_REQUEST>> GetAsync(string id)
        {
            if (id.IsNullOrEmpty()) return AjaxResult<DRUG_REQUEST>.Error("请传递参数");
            return AjaxResult<DRUG_REQUEST>.Success(await _service.GetAsync(id), "成功");
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveAsync(SaveRequest<DRUG_REQUEST> request)
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

        /// <summary>
        /// 撤销提交
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> RevokeAsync(string requestId)
        {
            return await _service.RevokeAsync(requestId);
        }
    }
}
