using Chloe;
using DocumentFormat.OpenXml.Drawing.Charts;
using EAM.Device.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAM.Device.Services
{
    public class DeviceVaryService : IDeviceVaryService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly ICorpService _corpService;

        public DeviceVaryService(IDbContext dbContext, IComboxDataService comboxDataService, ICorpService corpService)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _corpService = corpService;
        }

        /// <summary>
        /// 下拉
        /// </summary>
        /// <returns></returns>
        public async Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData()
        {
            var result = await _comboxDataService.Get(new Dictionary<string, object>(){
                    {"VaryType",null }
            });
            result.TryAdd("Corp", await _corpService.ComboxDataAsync());
            return result;
        }

        public async Task<GridData> ListAsync(GridRequest request)
        {
            var query = await _dbContext.Query<DEVICE_VARY>().GetGridData(request);
            return query;
        }

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
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(DEVICE_VARY entity)
        {
            string type = string.Empty;
            string def = string.Empty;
            string model = string.Empty;
            int index;

            entity.VARY_ID = GuidHelper.NewSnowflakeId().ToString();
            #region 根据变动类型保存变动单号
            switch (entity.VARY_TYPE)
            {
                case "trans":
                    type = "DB" + DateTime.Now.ToString("yyMM");
                    def = type + "0000";
                    model = await _dbContext.Query<DEVICE_VARY>(x => x.VARY_CODE.Contains(type)).Select(x => Sql.Max(x.VARY_CODE) ?? def).FirstOrDefaultAsync();
                    break;
                case "disable":
                    type = "ST" + DateTime.Now.ToString("yyMM");
                    def = type + "0000";
                    model = await _dbContext.Query<DEVICE_VARY>(x => x.VARY_CODE.Contains(type)).Select(x => Sql.Max(x.VARY_CODE) ?? def).FirstOrDefaultAsync();
                    break;
                case "enable":
                    type = "TQ" + DateTime.Now.ToString("yyMM");
                    def = type + "0000";
                    model = await _dbContext.Query<DEVICE_VARY>(x => x.VARY_CODE.Contains(type)).Select(x => Sql.Max(x.VARY_CODE) ?? def).FirstOrDefaultAsync();
                    break;
                case "idle":
                    type = "XZ" + DateTime.Now.ToString("yyMM");
                    def = type + "0000";
                    model = await _dbContext.Query<DEVICE_VARY>(x => x.VARY_CODE.Contains(type)).Select(x => Sql.Max(x.VARY_CODE) ?? def).FirstOrDefaultAsync();
                    break;
                case "unidle":
                    type = "XQ" + DateTime.Now.ToString("yyMM");
                    def = type + "0000";
                    model = await _dbContext.Query<DEVICE_VARY>(x => x.VARY_CODE.Contains(type)).Select(x => Sql.Max(x.VARY_CODE) ?? def).FirstOrDefaultAsync();
                    break;
                case "scrap":
                    type = "BF" + DateTime.Now.ToString("yyMM");
                    def = type + "0000";
                    model = await _dbContext.Query<DEVICE_VARY>(x => x.VARY_CODE.Contains(type)).Select(x => Sql.Max(x.VARY_CODE) ?? def).FirstOrDefaultAsync();
                    break;
                case "allot":
                    type = "BM" + DateTime.Now.ToString("yyMM");
                    def = type + "0000";
                    model = await _dbContext.Query<DEVICE_VARY>(x => x.VARY_CODE.Contains(type)).Select(x => Sql.Max(x.VARY_CODE) ?? def).FirstOrDefaultAsync();
                    break;
            }
            #endregion
            index = model.SubStr(6, 4).CastTo<int>() + 1;
            entity.VARY_CODE = type + index.ToString("D4");

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
