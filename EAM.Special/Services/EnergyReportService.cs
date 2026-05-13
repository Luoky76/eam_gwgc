using Chloe;
using Gksyb.Common;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Core.Interfaces.Auth;
using System.Collections.Concurrent;
using Gksyb.Model.UI;

namespace EAM.Special.Services
{
    public class EnergyReportService : BaseService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxService;
        private readonly UserSession _userSession;
        private readonly ICorpService _corpService;
        private DateTime? _Sysdate;

        public EnergyReportService(IDbContext dbContext, IComboxDataService comboxService, UserSession userSession, ICorpService corpService)
        {
            _dbContext = dbContext;
            _comboxService = comboxService;
            _userSession = userSession;
            _corpService = corpService;
        }

        private DateTime? Sysdate
        {
            get
            {
                if (!_Sysdate.HasValue)
                {
                    _Sysdate = _dbContext.GetSysdate().Result();
                }
                return _Sysdate;
            }
        }

        public async Task<AjaxResult> ComboxDataAsync()
        {
            var data = new ConcurrentDictionary<string, List<ComboxData>>();
            data.TryAdd("corpData", await _corpService.ComboxDataAsync());

            return AjaxResult.Success(data);
        }

        /// <summary>
        /// 根据ID获取记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<REPORT_ENERGY> GetAsync(object id)
        {
            string sid = id.ToString();
            var query = await _dbContext.Query<REPORT_ENERGY>().Where(c => c.MAINKEY == sid).FirstAsync();
            return query;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> GridListAsync(GridRequest request)
        {
            return await _dbContext.Query<REPORT_ENERGY>()
                .GetGridData(request);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAsync(SaveRequest<REPORT_ENERGY> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.MAINKEY,
                    c.COMPANY_ID,
                    c.COMPANY_NAME,
                    c.REPORT_ID,
                    c.PRODUCTION_DIESEL_OIL_TONS,
                    c.PRODUCTION_DIESEL_OIL_MONEY,
                    c.PRODUCTION_GASOLINE_TONS,
                    c.PRODUCTION_GASOLINE_MONEY,
                    c.PRODUCTION_HEAVY_OIL_TONS,
                    c.PRODUCTION_HEAVY_OIL_MONEY,
                    c.PRODUCTION_ELECTRIC_TONS,
                    c.PRODUCTION_ELECTRIC_MONEY,
                    c.PRODUCTION_NATURAL_GAS_TONS,
                    c.PRODUCTION_NATURAL_GAS_MONEY,
                    c.PRODUCTION_TOTAL_TONS,
                    c.PRODUCTION_TOTAL_MONEY,
                    c.NONPRODUCTION_DIESEL_OIL_TONS,
                    c.NONPRODUCTION_DIESEL_OIL_MONEY,
                    c.NONPRODUCTION_GASOLINE_TONS,
                    c.NONPRODUCTION_GASOLINE_MONEY,
                    c.NONPRODUCTION_HEAVY_OIL_TONS,
                    c.NONPRODUCTION_HEAVY_OIL_MONEY,
                    c.NONPRODUCTION_ELECTRIC_TONS,
                    c.NONPRODUCTION_ELECTRIC_MONEY,
                    c.NONPRODUCTION_TOTAL_TONS,
                    c.NONPRODUCTION_TOTAL_MONEY,
                    c.COMPRE_DIESEL_OIL_TONS,
                    c.COMPRE_DIESEL_OIL_MONEY,
                    c.COMPRE_GASOLINE_TONS,
                    c.COMPRE_GASOLINE_MONEY,
                    c.COMPRE_HEAVY_OIL_TONS,
                    c.COMPRE_HEAVY_OIL_MONEY,
                    c.COMPRE_ELECTRIC_TONS,
                    c.COMPRE_ELECTRIC_MONEY,
                    c.COMPRE_NATURAL_GAS_TONS,
                    c.COMPRE_NATURAL_GAS_MONEY,
                    c.COMPRE_TOTAL_TONS,
                    c.COMPRE_TOTAL_MONEY,
                    c.FINISH_PEOPLE_NUM,
                    c.FINISH_TONS,
                    c.FINISH_MONEY,
                    c.COMPRE_COAL_C,
                    c.COMPRE_COAL_D,
                    c.COMPRE_COAL_E,
                    c.OTHER_ELECTRIC_WATT_HOUR,
                    c.OTHER_ELECTRIC_TONS,
                    c.OTHER_OIL_TONS,
                    c.OTHER_OIL_TONS_COAL,
                    c.OTHER_TOTAL,
                    c.OTHER_INCOME,
                    c.OTHER_OUTPUT_VALUE,
                    c.ADD_USERID,
                    c.ADD_DATE,
                    c.ADD_USERNAME,
                    c.MODIFY_USERID,
                    c.MODIFY_DATE,
                    c.MODIFY_USERNAME,
                    c.STATISTICAL_MONTH,
                    c.TYPE,
                    c.REMARK
                },
                c => a => a.MAINKEY == c.MAINKEY
                , BeforeAdd, BeforeUpdate, BeforeDelete, false, null, AfterSave);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(REPORT_ENERGY entity)
        {
            entity.MAINKEY = GuidHelper.NewSnowflakeId().ToString();
            entity.COMPANY_ID = "800502";
            entity.COMPANY_NAME = "疏浚工程";
            entity.REPORT_ID = entity.MAINKEY;
            entity.ADD_DATE = Sysdate;
            entity.ADD_USERID = _userSession.UserID.ToString();
            entity.ADD_USERNAME = _userSession.RealName;
            entity.MODIFY_DATE = Sysdate;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFY_USERNAME = _userSession.RealName;

            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(REPORT_ENERGY entity)
        {
            entity.MODIFY_DATE = Sysdate;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFY_USERNAME = _userSession.RealName;

            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(REPORT_ENERGY entity)
        {
            await Task.CompletedTask;
        }
        private async Task AfterSave(List<REPORT_ENERGY> added, List<REPORT_ENERGY> updated, List<REPORT_ENERGY> deleted)
        {

            await Task.CompletedTask;
        }

    }
}
