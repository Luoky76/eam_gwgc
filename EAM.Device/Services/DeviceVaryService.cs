using Chloe;
using Gksyb.Common;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Core.Interfaces.General;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System.Collections.Concurrent;
using Gksyb.Core.Auth;

namespace EAM.Device.Services
{
    public class DeviceVaryService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly ICorpService _corpService;
        private readonly ICodeCreatorService _codeCreatorService;

        public DeviceVaryService(IDbContext dbContext, IComboxDataService comboxDataService, ICorpService corpService, ICodeCreatorService codeCreatorService)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _corpService = corpService;
            _codeCreatorService = codeCreatorService;
        }

        /// <summary>
        /// 下拉
        /// </summary>
        /// <returns></returns>
        public async Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxDataAsync()
        {
            var result = await _comboxDataService.Get(new Dictionary<string, object>(){
                    {"VaryType",null }
            });
            result.TryAdd("Corp", await _corpService.ComboxDataAsync());
            return result;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var query = await _dbContext.Query<DEVICE_VARY>().GetGridData(request);
            return query;
        }

        /// <summary>
        /// 保存
        /// </summary>
        public async Task<AjaxResult> SaveAsync(SaveRequest<DEVICE_VARY> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING,
                    c.VARY_CODE,
                    c.EIDT_USER,
                    c.DEPT_ID,
                    c.WDEPT_ID,
                    c.DEPT_NAME,
                    c.WDEPT_NAME,
                    c.APPLY_DATE,
                    c.VARY_DATE,
                    c.VARY_TYPE,
                    c.SUM_ORG_VALUE,
                    c.SUM_NET_VALUE,
                    c.SUM_DEP_VALUE,
                    c.REASONS,
                    c.TECH_STATUS,
                    c.MEMO,
                    c.VARY_ID
                },
                c => a => a.VARY_ID == c.VARY_ID
                , BeforeAdd, BeforeUpdate, BeforeDelete);
        }

        /// <summary>
        /// 提交
        /// </summary>
        public async Task<AjaxResult> SubmitAsync(List<string> sids)
        {
            if (sids == null || sids.Count == 0) return AjaxResult.Error("请选择行");
            await _dbContext.UpdateAsync<DEVICE_VARY>(x => sids.Contains(x.VARY_ID),
                x => new DEVICE_VARY
                {
                    AUDITING = "1"
                });
            return AjaxResult.Success("提交成功");
        }


        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(DEVICE_VARY entity)
        {
            entity.VARY_ID = GuidHelper.NewSnowflakeId().ToString();

            if (entity.VARY_CODE.IsNullOrWhiteSpace())
            {
                string prefix = entity.VARY_TYPE switch
                {
                    "trans" => "DB",
                    "disable" => "ST",
                    "enable" => "TQ",
                    "idle" => "XZ",
                    "unidle" => "XQ",
                    "scrap" => "BF",
                    "allot" => "BM",
                    _ => ""
                };
                if (!prefix.IsNullOrEmpty())
                {
                    entity.VARY_CODE = await _codeCreatorService.CreateCodeAsync<DEVICE_VARY>(prefix, a => a.VARY_CODE);
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(DEVICE_VARY entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(DEVICE_VARY entity)
        {
            await Task.CompletedTask;
        }
    }
}
