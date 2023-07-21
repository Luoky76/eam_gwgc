using EAM.Material.DTO;
using EAM.Material.Interfaces;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controller
{
    [GksybAuthorize(MenuNo = "ProviderAssess")]
    public class ProviderAssessController : AreaController
    {
        private readonly IProviderAssessService _service;

        /// <summary>
        /// 供应商评估主表
        /// </summary>
        /// <param name="service"></param>
        public ProviderAssessController(IProviderAssessService service)
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
        /// 连接评估任务表后返回列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<GridData> ExtendListAsync(GridRequest request)
        {
            return await _service.ExtendListAsync(request);
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
        public async Task<AjaxResult<PROVIDER_ASSESS>> GetAsync(string id)
        {
            if (id.IsNullOrEmpty()) return AjaxResult<PROVIDER_ASSESS>.Error("请传递参数");
            return AjaxResult<PROVIDER_ASSESS>.Success(await _service.GetAsync(id), "成功");
        }

        /// <summary>
        /// 生成主键
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost]
        public AjaxResult<string> CreatePrimaryKey()
        {
            return AjaxResult<string>.Success(_service.CreatePrimaryKey(), "成功");
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
