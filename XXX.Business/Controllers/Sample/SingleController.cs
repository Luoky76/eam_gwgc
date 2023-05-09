using Gksyb.Core.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Gksyb.Model.XXX.Business;
using Microsoft.AspNetCore.Mvc;
using XXX.Business.Interfaces.Sample;

namespace XXX.Business.Controllers.Sample
{
    [GksybAuthorize(MenuNo = "Single*", IsRegex = true)]
    public class SingleController : AreaController
    {
        private readonly ISingleService _service;

        /// <summary>
        /// 单表管理
        /// </summary>
        /// <param name="service"></param>
        public SingleController(ISingleService service)
        {
            _service = service;
        }

        /// <summary>
        /// 下拉数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<AjaxResult> ComboxDataAsync([FromServices] ICommonService commonService, [FromServices] IComboxDataService comboxDataService)
        {
            var data = await comboxDataService.Get(new Dictionary<string, object>(){
                { "BCCode", "岗位" }
            });
            data.TryAdd("roleData", await _service.RoleData());
            data.TryAdd("corpData", await commonService.JsonValueAsync<ComboxData>(new Gksyb.Core.Common.QueryViewRequest()
            {
                ViewName = "CorpDataCommon"
            }));
            return AjaxResult.Success(data, "成功");
        }

        [HttpPost]
        public async Task<AjaxResult<SAMPLE_TABLE>> GetAsync(string id)
        {
            return AjaxResult<SAMPLE_TABLE>.Success(await _service.GetAsync(id));
        }

        [HttpPost]
        public async Task<AjaxResult<GridData>> ListAsync(GridRequest request)
        {
            return AjaxResult<GridData>.Success(await _service.ListAsync(request), "成功");
        }

        [HttpPost]
        [JsToken]
        public virtual async Task<AjaxResult> SaveAsync(SaveRequest<SAMPLE_TABLE> request)
        {
            var result = await ValidSaveAsync(request);
            if (result.IsError) return result;
            return await _service.SaveAsync(request);
        }
    }
}