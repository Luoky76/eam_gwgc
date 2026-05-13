using EAM.Material.Services;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controller
{
    //[GksybAuthorize(MenuNo = "ProviderAssessTask,ProviderAssessResult")]
    [GksybAuthorize(true)]
    public class ProviderAssessTaskController : AreaController
    {
        private readonly ProviderAssessTask _service;

        /// <summary>
        /// 供应商评估任务制定
        /// </summary>
        /// <param name="service"></param>
        public ProviderAssessTaskController(ProviderAssessTask service)
        {
            _service = service;
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<GridData>> ListAsync(GridRequest request)
        {
            var result = await _service.ListAsync(request);
            return AjaxResult<GridData>.Success(result);
        }

        /// <summary>
        /// 获取单行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<PROVIDER_ASSESS_TASK>> GetAsync(string id)
        {
            if (id.IsNullOrEmpty()) return AjaxResult<PROVIDER_ASSESS_TASK>.Error("请传递参数");
            return AjaxResult<PROVIDER_ASSESS_TASK>.Success(await _service.GetAsync(id), "成功");
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveAsync(SaveRequest<PROVIDER_ASSESS_TASK> request)
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
            return AjaxResult.Success(await _service.SubmitAsync(sids), "成功");
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

        /// <summary>
        /// 连接评估表PROVIDER_ASSESS
        /// 返回评估结果
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult<GridData>> ResultListAsync(GridRequest request)
        {
            var result = await _service.ResultListAsync(request);
            return AjaxResult<GridData>.Success(result);
        }

        /// <summary>
        /// 撤销提交
        /// </summary>
        /// <param name="assessTaskId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> RevokeAsync(string assessTaskId)
        {
            return await _service.RevokeAsync(assessTaskId);
        }
    }
}
