using Chloe;
using EAM.Repair.interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace EAM.Repair.services
{
    public class RepairPlanService : IRepairPlanService
    {
        private readonly IDbContext _dbContext;
        private readonly UserSession _userSession;
        private readonly IComboxDataService _comboxDataService;
        public RepairPlanService(IDbContext dbContext,UserSession userSession,IComboxDataService comboxDataService) 
        { 
            _dbContext = dbContext;
            _userSession = userSession;
            _comboxDataService = comboxDataService;
        }
        /// <summary>
        /// 下拉
        /// </summary>
        /// <returns></returns>
        public async Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData()
        {
            var result = await _comboxDataService.Get(new Dictionary<string, object>(){
                    {"ShipList",null },
                    {"MaintDept", null},
                    {"RepairType",null },
                    {"RepairDealType",null }
            });
            return result;
        }
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var query = await _dbContext.JoinQuery<REP_PLAN, DEVICE_CARD>((a, b) => new object[]
            {
                JoinType.LeftJoin,a.DEVICE_ID.Equals(b.DEVICE_ID)
            }).Select((a, b) => new
            {
                a.PLAN_ID,
                a.AUDITING,
                a.WSEC_DEPT,
                a.MAINT_TYPE,
                a.DEAL_TYPE,
                a.AUDIT_TIME,
                a.PLAN_START_DATE,
                a.PLAN_END_DATE,
                a.PLAN_CODE,
                a.DEPT_NAME,
                a.CHARGE_USER,
                a.PLAN_MEMO,
                a.EIDT_DATE,
                b.DEVICE_ID,
                b.DEVICE_NAME,
                b.DEVICE_CODE,
                b.ASSET_CODE,
                AUDITINGSORT = Case.When(a.AUDITING.Equals("6")).Then("1.5").Else(a.AUDITING)
            }).GetGridData(request);
            return query;
        }
        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> Save(SaveRequest<REP_PLAN> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                   c.AUDITING,
                   c.PLAN_CODE,
                   c.PLAN_STATE,
                   c.DEPT_NAME,
                   c.WSEC_DEPT,
                   c.MAINT_TYPE,
                   c.DEAL_TYPE,
                   c.FAULT_DESCRIBE,
                   c.PLAN_MEMO,
                   c.PLAN_START_DATE,
                   c.PLAN_END_DATE,
                   c.PLAN_STOP_TIME,
                   c.CHARGE_USER,
                   c.COLLECT_METHOD,
                   c.PLAN_MONEY,
                   c.REPAIR_MEMO
                },
                c => a => a.PLAN_ID == c.PLAN_ID, BeforeAdd, BeforeUpdate, BeforeDelete, false);
        }
        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        private async Task BeforeAdd(REP_PLAN entity)
        {
            entity.PLAN_ID = GuidHelper.NewSnowflakeId().ToString();
            await Task.CompletedTask;
        }
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(REP_PLAN request)
        {
            await Task.CompletedTask;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeDelete(REP_PLAN request)
        {
            await Task.CompletedTask;
        }
        /// <summary>
        /// 船舶列表
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ShipList()
        {
            var result = await _dbContext.Query<DEVICE_CARD>()
                .OrderBy(c => c.DEVICE_ID)
                .Select(c => new DEVICE_CARD { DEVICE_ID = c.DEVICE_ID, DEVICE_NAME = c.DEVICE_NAME, DEVICE_CODE = c.DEVICE_CODE, DEPT_NAME = c.DEPT_NAME, WSEC_DEPT = c.WSEC_DEPT })
               .ToListAsync();
            return AjaxResult.Success(result, "成功");
        }
    }   
}
