using EAM.Special.Services;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Special.Controller
{
    [GksybAuthorize(MenuNo = "DrugRequest")]
    public class DrugRequestDetController : AreaController
    {
        private readonly DrugRequestDetService _service;

        /// <summary>
        /// 药品需求明细表
        /// </summary>
        /// <param name="service"></param>
        public DrugRequestDetController(DrugRequestDetService service)
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
        public async Task<AjaxResult<DRUG_REQUEST_DET>> GetAsync(string id)
        {
            if (id.IsNullOrEmpty()) return AjaxResult<DRUG_REQUEST_DET>.Error("请传递参数");
            return AjaxResult<DRUG_REQUEST_DET>.Success(await _service.GetAsync(id), "成功");
        }

        /// <summary>
        /// 根据药品需求ID REQUEST_ID 获取多行记录
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<GridData> GetCertainRequestAsync(string requestId)
        {
            return await _service.GetCertainRequestAsync(requestId);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveAsync(SaveRequest<DRUG_REQUEST_DET> request)
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
