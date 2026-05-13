using EAM.Material.Services;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controller
{
    //[GksybAuthorize(MenuNo = "ProviderAssess,ProviderAssessTask")]
    [GksybAuthorize(true)]
    public class ProviderAssessController : AreaController
    {
        private readonly ProviderAssessService _service;

        /// <summary>
        /// 供应商评估主表
        /// </summary>
        /// <param name="service"></param>
        public ProviderAssessController(ProviderAssessService service)
        {
            _service = service;
        }

        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ListAsync(GridRequest request)
        {
            var result = await _service.ListAsync(request);
            return AjaxResult.Success(result);
        }

        /// <summary>
        /// 连接评估任务表后返回列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ExtendListAsync(GridRequest request)
        {
            var result = await _service.ExtendListAsync(request);
            return AjaxResult.Success(result);
        }

        /// <summary>
        /// 根据评估任务ID ASSESS_TASK_ID 返回列表
        /// </summary>
        /// <param name="assessTaskId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<GridData> GetCertainAssessTaskAsync(string assessTaskId)
        {
            return await _service.GetCertainAssessTaskAsync(assessTaskId);
        }

        /// <summary>
        /// 连接评估任务表后
        /// 根据评估ID ASSESS_ID 返回列表
        /// </summary>
        /// <param name="assessId"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> GetCertainAssessAsync(string assessId)
        {
            var result = await _service.GetCertainAssessAsync(assessId);
            return AjaxResult.Success(result);
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
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> SaveAsync(SaveRequest<PROVIDER_ASSESS> request)
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
        /// 撤销提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> RevokeAsync(List<string> sids)
        {
            return AjaxResult.Success(await _service.RevokeAsync(sids), "成功");
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
