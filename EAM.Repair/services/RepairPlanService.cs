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
        private readonly ICorpService _corpService;
        private string masterID = string.Empty, errMsg = string.Empty;
        public RepairPlanService(IDbContext dbContext, IComboxDataService comboxDataService, IUserService userService, ICorpService corpService)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userService = userService;
            _corpService = corpService;
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
                    { "User", null }
            });
            result.TryAdd("Corp", await _corpService.ComboxDataAsync());
            return result;
        }
        #region 维修计划

        public async Task<GridData> ListAsync(GridRequest request)
        {
            var query = await _dbContext.JoinQuery<REP_PLAN, DEVICE_CARD>((a, b) => new object[]
            {
                JoinType.LeftJoin,a.DEVICE_ID.Equals(b.DEVICE_ID)
            })
            .Select((a, b) => new
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
                b.DEVICE_TYPE,
                b.DEVICE_NO,
                b.ASSET_CODE,
                AUDITINGSORT = Case.When(a.AUDITING.Equals("6")).Then("1.5").Else(a.AUDITING)
            }).GetGridData(request);
            return query;
        }

        public async Task<AjaxResult> GetDetailAsync(string ID)
        {
            var query = await _dbContext.JoinQuery<REP_PLAN, DEVICE_CARD>((a, b) => new object[]
            {
                JoinType.LeftJoin,a.DEVICE_ID.Equals(b.DEVICE_ID)
            })
            .Select((a, b) => new
            {
                a.PLAN_ID,
                a.AUDITING,
                a.WSEC_DEPT,
                a.MAINT_TYPE,
                a.DEAL_TYPE,
                a.AUDIT_TIME,
                a.PLAN_START_DATE,
                a.PLAN_END_DATE,
                a.PLAN_STOP_TIME,
                a.PLAN_CODE,
                a.FAULT_DESCRIBE,
                a.COLLECT_METHOD,
                a.PLAN_MONEY,
                a.REPAIR_MEMO,
                a.DEPT_NAME,
                a.CHARGE_USER,
                a.PLAN_MEMO,
                a.EIDT_DATE,
                b.DEVICE_ID,
                b.DEVICE_NAME,
                b.DEVICE_TYPE,
                b.DEVICE_NO,
                b.INSTALL_SITE,
                b.ASSET_CODE,
                AUDITINGSORT = Case.When(a.AUDITING.Equals("6")).Then("1.5").Else(a.AUDITING)
            }).Where(x => x.PLAN_ID == ID).ToListAsync();

            return AjaxResult.Success(query);
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
                    c.AUDIT_TIME,
                    c.DEAL_TYPE,
                    c.FAULT_DESCRIBE,
                    c.PLAN_MEMO,
                    c.DEVICE_ID,
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

            string type = "WXJH" + DateTime.Now.ToString("yyyyMM");
            string def = type + "0000";
            var model = await _dbContext.Query<REP_PLAN>(x => x.PLAN_CODE.Contains(type)).Select(x => Sql.Max(x.PLAN_CODE) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(10, 4).CastTo<int>() + 1;
            entity.PLAN_CODE = type + index.ToString("D4");

            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(REP_PLAN request)
        {
            if (request.AUDITING == "1")
            {
                REP_PLAN_EXE exe = new();
                exe.AUDITING = "0";
                exe.PLAN_CODE = request.PLAN_CODE;
                exe.DEVICE_ID = request.DEVICE_ID;
                exe.MAINT_TYPE = request.MAINT_TYPE;
                exe.DEAL_TYPE = request.DEAL_TYPE;
                exe.REP_LEVEL = request.REP_LEVEL;
                exe.FAULT_DESCRIBE = request.FAULT_DESCRIBE;
                exe.PLAN_START_DATE = request.PLAN_START_DATE;
                exe.PLAN_END_DATE = request.PLAN_END_DATE;
                exe.CHARGE_USER = request.CHARGE_USER;
                exe.REPAIR_MEMO = request.REPAIR_MEMO;
                exe.PLAN_MEMO = request.PLAN_MEMO;
                exe.PLAN_ID = request.PLAN_ID;
                exe.EXE_ID = GuidHelper.NewSnowflakeId().ToString();
                request.AUDIT_TIME = DateTime.Now;

                string type = "WXSS" + DateTime.Now.ToString("yyyyMM");
                string def = type + "0000";
                var model = await _dbContext.Query<REP_PLAN_EXE>(x => x.EXE_CODE.Contains(type)).Select(x => Sql.Max(x.EXE_CODE) ?? def).FirstOrDefaultAsync();
                var index = model.SubStr(10, 4).CastTo<int>() + 1;
                exe.EXE_CODE = type + index.ToString("D4");

                var item = await _dbContext.Query<REP_PLAN_ITEM>(x => x.PLAN_ID == request.PLAN_ID).ToListAsync();

                foreach (var iten in item)
                {
                    REP_PLAN_EXE_ITEM exeitem = new();
                    exeitem.BOM_NAME = iten.BOM_NAME;
                    exeitem.REP_INDEX = iten.REP_INDEX;
                    exeitem.REP_CONTENT = iten.REP_CONTENT;
                    exeitem.DEAL_TYPE = iten.DEAL_TYPE;
                    exeitem.ITEM_TYPE = iten.ITEM_TYPE;
                    exeitem.IS_ASKBID = iten.IS_ASKBID;
                    exeitem.REP_LEADER = iten.REP_LEADER;
                    exeitem.PLAN_ID = iten.PLAN_ID;
                    exeitem.EXE_ITEM_ID = GuidHelper.NewSnowflakeId().ToString();
                    exeitem.PLAN_ITEM_ID = iten.PLAN_ITEM_ID;
                    exeitem.BOM_ID = iten.BOM_ID;
                    exeitem.EXE_ID = exe.EXE_ID;

                    await _dbContext.InsertAsync<REP_PLAN_EXE_ITEM>(exeitem);
                }

                await _dbContext.InsertAsync<REP_PLAN_EXE>(exe);
            }
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
            var result = await _dbContext.Query<DEVICE_CARD>(a=> a.TYPE_ID == "1")//设备类别为船舶
                .OrderBy(c => c.DEVICE_ID)
                .Select(c => new DEVICE_CARD { AUDITING = c.AUDITING, DEVICE_ID = c.DEVICE_ID, DEVICE_NAME = c.DEVICE_NAME, DEVICE_NO = c.DEVICE_NO, DEPT_NAME = c.DEPT_NAME, WSEC_DEPT = c.WSEC_DEPT, DEVICE_TYPE = c.DEVICE_TYPE })
               .ToListAsync();
            return AjaxResult.Success(result, "成功");
        }

        public async Task<GridData> ItemListAsync(GridRequest request)
        {
            var query = await _dbContext.JoinQuery<REP_PLAN_ITEM, REP_PLAN>((a,b) => new object[] {
                JoinType.LeftJoin,a.PLAN_ID.Equals(b.PLAN_ID)
            }).Select((a, b) => new 
            {
                a.PLAN_ITEM_ID,
                a.BOM_ID,
                a.PLAN_ID,
                a.BOM_NAME,
                a.REP_CONTENT,
                a.REP_METHOD,
                a.USE_TOOL,
                a.LABOR_NUM,
                a.TAKE_TIME,
                a.MEMO,
                a.DEAL_TYPE,
                a.REP_LEADER,
                a.REP_INDEX,
                a.IS_ASKBID,
                a.ITEM_TYPE,
                a.DEVICE_TYPE,
            }).GetGridData(request);

            return query;
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
                    a.BOM_NAME,
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
                a.PLAN_ID,
                a.AUDITING,
                a.AUDITING_A,
                a.EXE_CODE,
                a.CHECK_CODE,
                a.WSEC_DEPT,
                a.MAINT_TYPE,
                a.DEAL_TYPE,
                a.PLAN_START_DATE,
                a.PLAN_END_DATE,
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
                a.DEPT_NAME,
                a.CHARGE_USER,
                a.REPAIR_MEMO,
                a.EIDT_DATE,
                b.DEVICE_ID,
                b.DEVICE_NAME,
                b.DEVICE_TYPE,
                b.DEVICE_NO,
                b.ASSET_CODE,
                a.EXE_ID
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
                a.PLAN_ID,
                a.AUDITING,
                a.AUDITING_A,
                a.EXE_CODE,
                a.WSEC_DEPT,
                a.MAINT_TYPE,
                a.DEAL_TYPE,
                a.PLAN_START_DATE,
                a.PLAN_END_DATE,
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
                a.EXE_ID
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
                a.BOM_ID,
                a.PLAN_ID,
                a.BOM_NAME,
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
                         c.PLAN_ID,
                         c.AUDITING,
                         c.EXE_CODE,
                         c.MAINT_TYPE,
                         c.DEAL_TYPE,
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
                         c.CHARGE_USER,
                         c.REPAIR_MEMO,
                         c.EIDT_DATE,
                         c.DEVICE_ID,
                         c.CHECK_CODE,
                         c.CHECK_DESC,
                         c.CHECK_DATE,
                         c.CHECK_MEMO,
                         c.CHECK_USER,
                         c.EXE_ID
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
                             c.BOM_ID,
                             c.PLAN_ID,
                             c.BOM_NAME,
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
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(REP_PLAN_EXE request)
        {
            if (request.AUDITING == "1")
            {
                request.EIDT_DATE = DateTime.Now;
                request.AUDITING_A = "0";

                string type = "WXYS" + DateTime.Now.ToString("yyyyMM");
                string def = type + "0000";
                var model = await _dbContext.Query<REP_PLAN_EXE>(x => x.CHECK_CODE.Contains(type)).Select(x => Sql.Max(x.CHECK_CODE) ?? def).FirstOrDefaultAsync();
                var index = model.SubStr(10, 4).CastTo<int>() + 1;
                request.CHECK_CODE = type + index.ToString("D4");
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        private async Task BeforeAddDet(REP_PLAN_EXE_ITEM entity)
        {
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
                    c.BOM_ID,
                    c.PLAN_ID,
                    c.BOM_NAME,
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

        #region 维修计划验收

        public async Task<AjaxResult> SaveCheck(SaveRequest<REP_PLAN_EXE> request, SaveRequest<REP_PLAN_EXE_ITEM> requestdet)
        {
            using (var trans = _dbContext.BeginTransaction())  //事务保证保存数据的一致性
            {
                bool mainSuccess = false, detSuccess = false;
                var execResult = await _dbContext.SaveEntityAnsyc(request,
                     c => new
                     {
                         c.PLAN_ID,
                         c.AUDITING,
                         c.AUDITING_A,
                         c.EXE_CODE,
                         c.MAINT_TYPE,
                         c.DEAL_TYPE,
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
                         c.CHARGE_USER,
                         c.REPAIR_MEMO,
                         c.EIDT_DATE,
                         c.DEVICE_ID,
                         c.CHECK_CODE,
                         c.CHECK_DESC,
                         c.CHECK_DATE,
                         c.CHECK_MEMO,
                         c.CHECK_USER,
                         c.EXE_ID
                     },
                     c => a => a.EXE_ID == c.EXE_ID
                     , BeforeAddCHK, BeforeUpdateCHK);

                mainSuccess = !execResult.IsError;
                if (mainSuccess)  //主表是否保存成功
                {
                    requestdet = requestdet ?? new SaveRequest<REP_PLAN_EXE_ITEM>();

                    execResult = await _dbContext.SaveEntityAnsyc(requestdet,
                         c => new
                         {
                             c.EXE_ITEM_ID,
                             c.EXE_ID,
                             c.PLAN_ITEM_ID,
                             c.BOM_ID,
                             c.PLAN_ID,
                             c.BOM_NAME,
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
                         c => a => a.EXE_ITEM_ID == c.EXE_ITEM_ID,
                         BeforeAddDetCHK, BeforeUpdateDetCHK);

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
        private async Task BeforeAddCHK(REP_PLAN_EXE entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeUpdateCHK(REP_PLAN_EXE request)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        private async Task BeforeAddDetCHK(REP_PLAN_EXE_ITEM entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeUpdateDetCHK(REP_PLAN_EXE_ITEM request)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeDeleteDetCHK(REP_PLAN_EXE_ITEM request)
        {
            await Task.CompletedTask;
        }

        #endregion
    }
}