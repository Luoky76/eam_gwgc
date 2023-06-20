using Gksyb.Core.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Server.Services.System;
using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Server.Controllers.System
{
    /// <summary>
    /// 配置管理
    /// </summary>
    [GksybAuthorize(IsSuper = true)]
    public class TaskController : BaseController
    {
        private readonly TaskService _service;

        /// <summary>
        /// 配置管理
        /// </summary>
        /// <param name="service"></param>
        public TaskController(TaskService service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取下次运行时间
        /// </summary>
        /// <returns></returns>
        public AjaxResult NextFireTime(string cron)
        {
            var nextTime = _service.GetNextFireTimeUtc(cron);
            if (!nextTime.HasValue) return AjaxResult.Success("", "成功");
            var build = new StringBuilder();
            build.Append($"[{nextTime.Value.ToLocalTime().DateTime:yyyy-MM-dd HH:mm:ss}]");
            for (var i = 0; i < 2; i++)
            {
                nextTime = _service.GetNextFireTimeUtc(cron, nextTime);
                build.Append("  ");
                build.Append($"[{nextTime.Value.ToLocalTime().DateTime:yyyy-MM-dd HH:mm:ss}]");
            }
            return AjaxResult.Success(build.ToString(), "成功");
        }

        /// <summary>
        /// 获取单行数据
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult<SYS_TASK>> GetAsync(long? id)
        {
            if (!id.HasValue) return AjaxResult<SYS_TASK>.Error("请传递参数");
            return AjaxResult<SYS_TASK>.Success(await _service.GetAsync(id.Value), "成功");
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult<GridData>> ListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.ListAsync(request), "成功");
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        [JsToken]
        public async Task<AjaxResult> SaveAsync(SaveRequest<SYS_TASK> request)
        {
            return await _service.Save(request);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        [JsToken]
        public async Task<AjaxResult> Excute(List<SYS_TASK> request)
        {
            if (request.Any(c => c.TASK_STATUS != "正常")) return AjaxResult.Error("未启动的任务无法执行");
            await _service.Excute(request);
            return AjaxResult.Success();
        }
    }
}