using Chloe;
using DocumentFormat.OpenXml.Drawing.Charts;
using EAM.Repair.interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using NPOI.SS.Formula.PTG;
using System.Collections.Concurrent;
using static StackExchange.Redis.Role;

namespace EAM.Repair.services
{
    public class RepairPlanService : IRepairPlanService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly IUserService _userService;
        private readonly UserSession _userSession;
        private readonly ICorpService _corpService;
        private string _rentID = string.Empty, errMsg = string.Empty;
        public RepairPlanService(IDbContext dbContext, IComboxDataService comboxDataService, IUserService userService, ICorpService corpService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userService = userService;
            _corpService = corpService;
            _userSession = userSession;
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
                    {"RepitemType",null },
                    {"RepairDealType",null },
                    { "Auditing", null },
                    { "User", null },
                    { "PlanState", null },
            });
            result.TryAdd("Corp", await _corpService.ComboxDataAsync());
            return result;
        }
        #region 维修计划

        /// <summary>
        /// 船舶列表
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ShipList()
        {
            var result = await _dbContext.Query<DEVICE_CARD>(a => a.TYPE_ID == "1")//设备类别为船舶
                .OrderBy(c => c.DEVICE_ID)
                .Select(c => new DEVICE_CARD
                {
                    AUDITING = c.AUDITING,
                    DEVICE_ID = c.DEVICE_ID,
                    DEVICE_NAME = c.DEVICE_NAME,
                    DEVICE_NO = c.DEVICE_NO,
                    DEPT_NAME = c.DEPT_NAME,
                    DEPT_ID = c.DEPT_ID,
                    DEVICE_TYPE = c.DEVICE_TYPE,
                    INSTALL_SITE = c.INSTALL_SITE,
                })
               .ToListAsync();
            return AjaxResult.Success(result, "成功");
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> SaveItem(SaveRequest<REP_PLAN_ITEM> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                a => new
                {
                    a.PLAN_ITEM_ID,
                    a.PLAN_ID,
                    a.DEVICE_NAME,
                    a.REP_CONTENT,
                    a.MEMO,
                    a.DEAL_TYPE,
                    a.REP_LEADER,
                    a.REP_INDEX,
                    a.IS_ASKBID,
                    a.ITEM_TYPE,
                    a.DEVICE_TYPE,
                },
                c => a => a.PLAN_ITEM_ID == c.PLAN_ITEM_ID, BeforeAddItem, BeforeUpdateItem, BeforeDeleteItem, false);
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        private async Task BeforeAddItem(REP_PLAN_ITEM entity)
        {
            entity.PLAN_ITEM_ID = GuidHelper.NewSnowflakeId().ToString();

            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeUpdateItem(REP_PLAN_ITEM request)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeDeleteItem(REP_PLAN_ITEM request)
        {
            await Task.CompletedTask;
        }

        public async Task<GridData> GetDeviceAsync(GridRequest request)
        {
            var query = await _dbContext.Query<DEVICE_CARD>().Where(c => c.TYPE_ID == "2").GetGridData(request);
            return query;
        }
        #endregion

        #region 维修计划实施

        public async Task<GridData> ExeListAsync(GridRequest request)
        {
            var query = await _dbContext.JoinQuery<REP_PLAN_EXE, DEVICE_CARD>((a, b) => new object[]
            {
                JoinType.LeftJoin,a.DEVICE_ID.Equals(b.DEVICE_ID)
            })
            .Select((a, b) => new
            {
                a.AUDITING,
                a.AUDITING_A,
                a.AUDITING_B,
                a.AUDITING_D,
                a.EXE_CODE,
                a.CHECK_CODE,
                a.WSEC_DEPT,
                a.MAINT_TYPE,
                a.PLAN_STATE,
                a.DEAL_TYPE,
                a.PLAN_START_DATE,
                a.PLAN_END_DATE,
                a.PLAN_STOP_TIME,
                a.ACT_START_DATE,
                a.ACT_END_DATE,
                a.ACT_STOP_TIME,
                a.EXE_USER,
                a.ASSIST_USER,
                a.IS_LEAVE,
                a.EXE_DESC,
                a.LEAVE_MEMO,
                a.FAULT_DESCRIBE,
                a.REP_LEVEL,
                a.PLAN_CODE,
                a.PLAN_MEMO,
                a.AUDIT_USER,
                a.REPORT_USER,
                a.AUDIT_USERID,
                a.REPORT_USERID,
                a.DEPT_NAME,
                a.CHARGE_USER,
                a.REPAIR_MEMO,
                a.EIDT_DATE,
                b.DEVICE_ID,
                b.DEVICE_NAME,
                b.DEVICE_TYPE,
                b.DEVICE_NO,
                b.ASSET_CODE,
                a.EXE_ID,
                a.COLLECT_METHOD,
                a.PLAN_MONEY,
                a.CHECK_DATE,
            }).GetGridData(request);

            return query;
        }

        public async Task<AjaxResult> GetExeDetailAsync(string ID)
        {
            var query = await _dbContext.JoinQuery<REP_PLAN_EXE, DEVICE_CARD>((a, b) => new object[]
            {
                JoinType.LeftJoin,a.DEVICE_ID.Equals(b.DEVICE_ID)
            })
            .Select((a, b) => new
            {
                a.AUDITING,
                a.AUDITING_A,
                a.AUDITING_B,
                a.AUDITING_D,
                a.EXE_CODE,
                a.PLAN_STATE,
                a.WSEC_DEPT,
                a.MAINT_TYPE,
                a.DEAL_TYPE,
                a.AUDIT_USER,
                a.REPORT_USER,
                a.AUDIT_USERID,
                a.REPORT_USERID,
                a.PLAN_START_DATE,
                a.PLAN_END_DATE,
                a.PLAN_STOP_TIME,
                a.FAULT_DESCRIBE,
                a.REP_LEVEL,
                a.PLAN_CODE,
                a.PLAN_MEMO,
                a.DEPT_NAME,
                a.CHARGE_USER,
                a.REPAIR_MEMO,
                a.EIDT_DATE,
                a.CHECK_CODE,
                a.CHECK_DESC,
                a.CHECK_DATE,
                a.CHECK_MEMO,
                a.CHECK_USER,
                a.ACT_START_DATE,
                a.ACT_END_DATE,
                a.ACT_STOP_TIME,
                a.EXE_USER,
                a.ASSIST_USER,
                a.IS_LEAVE,
                a.EXE_DESC,
                a.LEAVE_MEMO,
                b.DEVICE_ID,
                b.DEVICE_NAME,
                b.DEVICE_TYPE,
                b.DEVICE_NO,
                b.ASSET_CODE,
                a.EXE_ID,
                a.COLLECT_METHOD,
                a.PLAN_MONEY,
            }).Where(x => x.EXE_ID == ID).ToListAsync();

            return AjaxResult.Success(query);
        }

        public async Task<GridData> ExeItemListAsync(GridRequest request)
        {
            var query = await _dbContext.JoinQuery<REP_PLAN_EXE_ITEM, REP_PLAN_EXE>((a, b) => new object[] {
                JoinType.LeftJoin,a.EXE_ID.Equals(b.EXE_ID)
            }).Select((a, b) => new
            {
                a.EXE_ITEM_ID,
                a.EXE_ID,
                a.DEVICE_ID,
                a.PLAN_ID,
                a.DEVICE_NAME,
                a.REP_CONTENT,
                a.IS_COMPLETE,
                a.USE_TOOL,
                a.LABOR_NUM,
                a.TAKE_TIME,
                a.BEGIN_TIME,
                a.END_TIME,
                a.MEMO,
                a.DEAL_TYPE,
                a.REP_LEADER,
                a.REP_INDEX,
                a.IS_ASKBID,
                a.ITEM_TYPE,
                a.DEVICE_NO,
                a.DEVICE_SIZE,
                a.DEVICE_TYPE,
                a.DEVICE_NUM,
                a.STOCK_NAME,
            }).GetGridData(request);

            return query;
        }

        public async Task<AjaxResult> SaveExe(SaveRequest<REP_PLAN_EXE> request, SaveRequest<REP_PLAN_EXE_ITEM> requestdet)
        {
            using (var trans = _dbContext.BeginTransaction())  //事务保证保存数据的一致性
            {
                bool mainSuccess = false, detSuccess = false;
                var execResult = await _dbContext.SaveEntityAnsyc(request,
                     c => new
                     {
                         c.AUDITING,
                         c.AUDITING_A,
                         c.AUDITING_B,
                         c.AUDITING_D,
                         c.EXE_CODE,
                         c.MAINT_TYPE,
                         c.DEAL_TYPE,
                         c.PLAN_STATE,
                         c.ACT_START_DATE,
                         c.ACT_END_DATE,
                         c.ACT_STOP_TIME,
                         c.EXE_USER,
                         c.ASSIST_USER,
                         c.IS_LEAVE,
                         c.EXE_DESC,
                         c.LEAVE_MEMO,
                         c.FAULT_DESCRIBE,
                         c.REP_LEVEL,
                         c.AUDIT_USER,
                         c.REPORT_USER,
                         c.AUDIT_USERID,
                         c.REPORT_USERID,
                         c.CHARGE_USER,
                         c.REPAIR_MEMO,
                         c.EIDT_DATE,
                         c.DEVICE_ID,
                         c.CHECK_CODE,
                         c.CHECK_DESC,
                         c.CHECK_DATE,
                         c.CHECK_MEMO,
                         c.CHECK_USER,
                         c.EXE_ID,
                         c.DEPT_NAME,
                         c.WSEC_DEPT,
                         c.PLAN_MEMO,
                         c.PLAN_START_DATE,
                         c.PLAN_END_DATE,
                         c.PLAN_STOP_TIME,
                         c.COLLECT_METHOD,
                         c.PLAN_MONEY,
                     },
                     c => a => a.EXE_ID == c.EXE_ID
                     , BeforeAdd, BeforeUpdate);

                mainSuccess = !execResult.IsError;
                if (mainSuccess)  //主表是否保存成功
                {
                    requestdet = requestdet ?? new SaveRequest<REP_PLAN_EXE_ITEM>();

                    execResult = await _dbContext.SaveEntityAnsyc(requestdet,
                         c => new
                         {
                             c.EXE_ITEM_ID,
                             c.EXE_ID,
                             c.DEVICE_ID,
                             c.PLAN_ID,
                             c.DEVICE_NAME,
                             c.REP_CONTENT,
                             c.IS_COMPLETE,
                             c.USE_TOOL,
                             c.LABOR_NUM,
                             c.TAKE_TIME,
                             c.BEGIN_TIME,
                             c.END_TIME,
                             c.MEMO,
                             c.DEAL_TYPE,
                             c.REP_LEADER,
                             c.REP_INDEX,
                             c.IS_ASKBID,
                             c.ITEM_TYPE,
                             c.DEVICE_NO,
                             c.DEVICE_SIZE,
                             c.DEVICE_TYPE,
                             c.DEVICE_NUM,
                             c.STOCK_NAME,
                         },
                         c => a => a.EXE_ITEM_ID == c.EXE_ITEM_ID,
                         BeforeAddDet, BeforeUpdateDet);

                    detSuccess = !execResult.IsError;  //明细表是否保存成功
                }
                if (mainSuccess && detSuccess)
                    trans.Commit();
                else
                {
                    trans.Rollback();
                    if (string.IsNullOrWhiteSpace(errMsg)) errMsg = "保存失败";
                    return AjaxResult.Error(errMsg);
                }
            }
            return AjaxResult.Success("保存成功");
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        private async Task BeforeAdd(REP_PLAN_EXE entity)
        {
            if (entity.AUDITING_A == "0")
            {
                entity.EXE_ID = _rentID =GuidHelper.NewSnowflakeId().ToString();
                //request.AUDIT_TIME = DateTime.Now;

                entity.REPORT_USER = _userSession.UserName;
                entity.REPORT_USERID = _userSession.UserID.ToString();
                string type = "WXSB" + DateTime.Now.ToString("yyyyMM");
                string def = type + "0000";
                var model = await _dbContext.Query<REP_PLAN_EXE>(x => x.EXE_CODE.Contains(type)).Select(x => Sql.Max(x.EXE_CODE) ?? def).FirstOrDefaultAsync();
                var index = model.SubStr(10, 4).CastTo<int>() + 1;
                entity.EXE_CODE = type + index.ToString("D4");

            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(REP_PLAN_EXE request)
        {
            if (request.AUDITING_B == "0" && request.AUDITING == null)
            {
                request.AUDIT_USER = _userSession.UserName;
                request.AUDIT_USERID = _userSession.UserID.ToString();
            }
            if (request.AUDITING_A == null && request.AUDITING_B == null)
            {
                request.AUDITING_A = "0";
                request.PLAN_STATE = "10"; // 故障上报
            }
            if (request.AUDITING == "0" && request.AUDITING_D == null)
            {
                request.EXE_USER = _userSession.UserName;
                request.EXE_USERID = _userSession.UserID.ToString();
            }
            if (request.AUDITING_A == "1" && request.AUDITING_B == null)
            {
                request.AUDITING_B = "0";
                request.PLAN_STATE = "20"; // 故障待审
            }
            if (request.AUDITING_B == "1" && request.AUDITING == null)
            {
                request.AUDITING = "0";
                request.PLAN_STATE = "30"; // 待实施
            }
            if (request.AUDITING == "1" && request.AUDITING_D == null)
            {
                var qry = _dbContext.Query<REP_PLAN_EXE_ITEM>(c=>c.EXE_ID == request.EXE_ID).Select(c=>c.IS_COMPLETE).ToList();
                if (qry.Contains(null))
                {
                    throw new MessageException("请确认是否完成");
                }
                // request.EIDT_DATE = DateTime.Now;
                request.AUDITING_D = "0";
                request.PLAN_STATE = "40"; // 待验收
/*
                string type = "WXYS" + DateTime.Now.ToString("yyyyMM");
                string def = type + "0000";
                var model = await _dbContext.Query<REP_PLAN_EXE>(x => x.CHECK_CODE.Contains(type))
                    .Select(x => Sql.Max(x.CHECK_CODE) ?? def).FirstOrDefaultAsync();
                var index = model.SubStr(10, 4).CastTo<int>() + 1;
                request.CHECK_CODE = type + index.ToString("D4");*/
            }
            if (request.AUDITING_D == "1")
            {
                request.PLAN_STATE = "50"; // 已验收
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        private async Task BeforeAddDet(REP_PLAN_EXE_ITEM entity)
        {
            entity.EXE_ID = string.IsNullOrWhiteSpace(entity.EXE_ID) ? _rentID : entity.EXE_ID;
            entity.EXE_ITEM_ID = GuidHelper.NewSnowflakeId().ToString();
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeUpdateDet(REP_PLAN_EXE_ITEM request)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeDeleteDet(REP_PLAN_EXE_ITEM request)
        {
            await Task.CompletedTask;
        }

        public async Task<AjaxResult> SaveExeItem(SaveRequest<REP_PLAN_EXE_ITEM> requestdet)
        {
            return await _dbContext.SaveEntityAnsyc(requestdet,
                c => new
                {
                    c.EXE_ITEM_ID,
                    c.EXE_ID,
                    c.PLAN_ITEM_ID,
                    c.DEVICE_ID,
                    c.PLAN_ID,
                    c.DEVICE_NAME,
                    c.REP_CONTENT,
                    c.IS_COMPLETE,
                    c.USE_TOOL,
                    c.LABOR_NUM,
                    c.TAKE_TIME,
                    c.BEGIN_TIME,
                    c.END_TIME,
                    c.MEMO,
                    c.DEAL_TYPE,
                    c.REP_LEADER,
                    c.REP_INDEX,
                    c.IS_ASKBID,
                    c.ITEM_TYPE,
                },
                c => a => a.EXE_ITEM_ID == c.EXE_ITEM_ID, BeforeAddDet, BeforeUpdateDet);
        }




        #endregion

    }
}