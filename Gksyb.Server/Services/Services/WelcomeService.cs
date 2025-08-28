using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Server.Interfaces.Welcome;
using Gksyb.Server.Services.Services.Dto;

namespace Gksyb.Server.Services.Message
{
    /// <summary>
    /// 消息
    /// </summary>
    public class WelcomeService : IWelcomeService
    {
        private readonly IDbContext _dbContext;
        private readonly UserSession _user;
        private bool allDataShow = true;


        public WelcomeService(IDbContext dbContext, UserSession userSession)
        {
            _dbContext = dbContext;
            _user = userSession;
            if (userSession.Corp.CName!="船机部")
            {
                allDataShow = false;
            }
        }
        #region 待办数据处理
        public async Task<GetTodoListDataCountResponse> GetTodoListData()
        {
            //维保部门 WSEC_DEPT ，设备名称
            GetTodoListDataCountResponse result = new GetTodoListDataCountResponse();
            result.exe = await _dbContext.Query<REP_PLAN_EXE>(x => (x.AUDITING_B == "1" || x.AUDITING_B == "3") && x.AUDITING_C == "0").WhereIf(allDataShow, x => x.DEPT_NAME == _user.ParentCompany.CName).CountAsync();
            result.exeTitle = "维修实施";
            result.exeList = await _dbContext.Query<REP_PLAN_EXE>(x => (x.AUDITING_B == "1" || x.AUDITING_B == "3") && x.AUDITING_C == "0")
                                               .WhereIf(allDataShow, x => x.DEPT_NAME == _user.ParentCompany.CName)
                                               .LeftJoin<DEVICE_CARD>((a, b) => a.DEVICE_ID == b.DEVICE_ID)
                                               .Select((a, b) => new todolist
                                               {
                                                   ID = a.EXE_ID,
                                                   TEXT = a.EXE_CODE + "," + a.WDEPT_NAME??" " + "," + b.DEVICE_NAME + "," + b.DEVICE_NO,
                                                   TYPENAME = "维修实施",
                                                   MENUNAME = "exe",
                                                   IDKEY = "EXE_ID",
                                               }).ToListAsync();



            result.check = await _dbContext.Query<REP_PLAN_EXE>(x => (x.AUDITING_C == "1" || x.AUDITING_C == "3") && x.AUDITING_D == "0").WhereIf(allDataShow, x => x.DEPT_NAME == _user.ParentCompany.CName).CountAsync();
            result.checkTitle = "维修验收";
            result.checkList = await _dbContext.Query<REP_PLAN_EXE>(x => (x.AUDITING_C == "1" || x.AUDITING_C == "3") && x.AUDITING_D == "0")
                                                .WhereIf(allDataShow, x => x.DEPT_NAME == _user.ParentCompany.CName)
                                                .LeftJoin<DEVICE_CARD>((a, b) => a.DEVICE_ID == b.DEVICE_ID)
                                                .Select((a, b) => new todolist
                                                {
                                                    ID = a.EXE_ID,
                                                    TEXT = a.EXE_CODE + "," + (a.WDEPT_NAME ?? " ") + "," + b.DEVICE_NAME + "," + b.DEVICE_NO,
                                                    TYPENAME = "维修验收",
                                                    MENUNAME = "check",
                                                    IDKEY = "EXE_ID",
                                                }).ToListAsync();

            result.RepDockExe = await _dbContext.Query<REP_DOCK_PLAN>(x => x.AUDITING_PLAN == "1").WhereIf(allDataShow, x => x.DEPT_NAME == _user.ParentCompany.CName).CountAsync();
            result.RepDockExeTitle = "码头维修实施";
            result.RepDockExeList = await _dbContext.Query<REP_DOCK_PLAN>(x => x.AUDITING_PLAN == "1")
                                                    .WhereIf(allDataShow, x => x.DEPT_NAME == _user.ParentCompany.CName)
                                                    .Select(x => new todolist
                                                    {
                                                        ID = x.PLAN_ID,
                                                        TEXT = x.EXE_CODE + "," + (x.DEPT_NAME ?? " ") + "," + x.DOCK_NAME + "," + x.REP_DESC,
                                                        TYPENAME = "码头维修实施",
                                                        MENUNAME = "RepDockExe",
                                                        IDKEY = "PLAN_ID"
                                                    }).ToListAsync();

            result.RepDockConfirm = await _dbContext.Query<REP_DOCK_CHECK>().WhereIf(allDataShow, x => x.DEPT_NAME == _user.ParentCompany.CName).CountAsync();
            result.RepDockConfirmTitle = "码头维修确认";
            //  .WhereIf(!_userSession.IsAdmin, a => _userSession.Corp.CorpID == a.DEPT_ID)
            result.RepDockConfirmList = await _dbContext.Query<REP_DOCK_CHECK>().WhereIf(allDataShow, x => x.DEPT_NAME == _user.ParentCompany.CName)
                                                                         .Select(x => new todolist
                                                                         {
                                                                             ID = x.CHECK_ID,
                                                                             TEXT = x.EXE_CODE + "," + (x.DEPT_NAME ?? " ") + "," + x.REP_ITEM + "," + x.MEMO,
                                                                             TYPENAME = "码头维修确认",
                                                                             MENUNAME = "RepDockConfirm",
                                                                             IDKEY = "CHECK_ID"
                                                                         }).ToListAsync();

            result.PmPlanExe = await _dbContext.Query<PM_PLAN_EXE>(x => x.AUDITING == "1").WhereIf(allDataShow, x => x.DEPT_NAME == _user.ParentCompany.CName).CountAsync();
            result.PmPlanExeTitle = "维保实施";
            result.PmPlanExeList = await _dbContext.Query<PM_PLAN_EXE>(x => x.AUDITING == "1")
                                                   .WhereIf(allDataShow, x => x.DEPT_NAME == _user.ParentCompany.CName)
                                                   .LeftJoin<DEVICE_CARD>((a, b) => a.DEVICE_ID == b.DEVICE_ID)
                                                   .Select((a, b) => new todolist
                                                   {
                                                       ID = a.EXE_ID,
                                                       TEXT = a.EXE_CODE + "," + (a.WDEPT_NAME ?? " ") + "," + b.DEVICE_NAME + "," + b.DEVICE_NO,
                                                       TYPENAME = "维保实施",
                                                       MENUNAME = "PmPlanExe",
                                                       IDKEY = "EXE_ID"

                                                   }).ToListAsync();


            return result;

        }





        #endregion

        #region 顶部数据相关接口
        /// <summary>
        /// 
        /// </summary>
        /// <param name="datetime"></param>
        /// <returns></returns>
        public async Task<GetDeviceRepairCountResponse> GetDeviceRepairCount(DateTime datetime)
        {
            GetDeviceRepairCountResponse result = new GetDeviceRepairCountResponse();
            result.deviceCount = await _dbContext.Query<DEVICE_CARD>(x => x.TYPE_NAME == "船舶").CountAsync();
            result.repairCount = await _dbContext.Query<REP_PLAN_EXE>(x => x.DEAL_TYPE == "自修" && x.AUDITING_D == "0").CountAsync();
            result.shiprepairCount = await _dbContext.Query<REP_DOCK_PLAN>(x => x.AUDITING_PLAN == "0").CountAsync();

            return result;
        }


        #endregion

        #region echart相关接口
        public async Task<GetDeviceRepairInfoEchartResponse> GetDeviceRepairInfoEchart()
        {
            DateTime nowTime = _dbContext.GetSysdate().Result().Value;
            DateTime startTime = new DateTime(nowTime.Year, 1, 1);
            DateTime EndTime = new DateTime(nowTime.Year, 12, 31);
            var query = await _dbContext.Query<REP_PLAN_EXE>(x => x.AUDITING_D == "1" && x.ACT_END_DATE.HasValue)
                .LeftJoin<DEVICE_CARD>((a, b) => a.DEVICE_ID == b.DEVICE_ID)
                .Select((a, b) => new
                {
                    b.DEVICE_NAME,
                    a.ACT_END_DATE,
                    a.ACT_STOP_TIME,
                    a.DEVICE_ID,
                    a.EXE_ID
                })
                .GroupBy(e => new { e.DEVICE_ID, e.DEVICE_NAME, e.ACT_END_DATE.Value })
                .Select(g => new
                {

                    Month = g.ACT_END_DATE.Value.Month,
                    g.DEVICE_NAME,
                    g.DEVICE_ID,
                    RepairCount = Sql.Count(g.EXE_ID),
                    TotalHours = (Sql.Sum(g.ACT_STOP_TIME) ?? (decimal)0.00) / (decimal)60.00 // 转换为小时
                }).ToListAsync();

            var query2 = await _dbContext.Query<REP_FAULT>(x => x.AUDITING == "1" && x.COMPLETE_DATE.HasValue)
                .LeftJoin<DEVICE_CARD>((a, b) => a.DEVICE_ID == b.DEVICE_ID)
                .Select((a, b) => new
                {
                    b.DEVICE_NAME,
                    a.COMPLETE_DATE,
                    a.REPAIR_HOURS,
                    a.DEVICE_ID,
                    a.FAULT_ID
                })
                .GroupBy(e => new { e.DEVICE_ID, e.DEVICE_NAME, e.COMPLETE_DATE.Value })
                .Select(g => new
                {

                    Month = g.COMPLETE_DATE.Value.Month,
                    g.DEVICE_NAME,
                    g.DEVICE_ID,
                    RepairCount = Sql.Count(g.FAULT_ID),
                    TotalHours = (Sql.Sum(g.REPAIR_HOURS) ?? (decimal)0.00) / (decimal)60.00
                }).ToListAsync();

            var combime = query.Concat(query2).ToList();
            //合并统计
            var sumList = combime.GroupBy(g => new { g.Month })
                 .Select(x => new
                 {
                     x.Key.Month,
                     RepairCount = x.Sum(x => x.RepairCount),
                     TotalHours = x.Sum(x => x.TotalHours)
                 }).ToList();

            List<decimal> hourList = new List<decimal>();
            List<int> RepairList = new List<int>();
            for (int i = 1; i <= 12; i++)
            {
                var info = sumList.Where(x => x.Month == i).FirstOrDefault();
                hourList.Add(info != null ? info.TotalHours : 0);
                RepairList.Add(info != null ? info.RepairCount : 0);
            }
            GetDeviceRepairInfoEchartResponse result = new GetDeviceRepairInfoEchartResponse();
            result.HourList = hourList;
            result.RepairList = RepairList;
            return result;

        }

        public async Task<GetDeviceInfoInMonthResponse> GetDeviceInfoInMonth(int month)
        {
            var Now = await _dbContext.GetSysdate();

            var startTime = new DateTime(Now.Value.Year, month, 1);
            var endTime = startTime.AddMonths(1).AddMilliseconds(-1);
            var query = await _dbContext.Query<REP_PLAN_EXE>(x => x.AUDITING_D == "1" && x.ACT_END_DATE >= startTime && x.ACT_END_DATE <= endTime)
                                        .LeftJoin<DEVICE_CARD>((a, b) => a.DEVICE_ID == b.DEVICE_ID)
                                        .Select((a, b) => new
                                        {
                                            b.DEVICE_NAME,
                                            a.ACT_STOP_TIME,
                                            a.DEVICE_ID,
                                            a.EXE_ID
                                        })
                                        .GroupBy(e => new { e.DEVICE_ID, e.DEVICE_NAME })
                                        .Select(g => new
                                        {
                                            g.DEVICE_NAME,
                                            g.DEVICE_ID,
                                            RepairCount = Sql.Count(g.EXE_ID),
                                            TotalHours = (Sql.Sum(g.ACT_STOP_TIME) ?? (decimal)0.00) / (decimal)60.00 // 转换为小时
                                        }).ToListAsync();
            GetDeviceInfoInMonthResponse result = new GetDeviceInfoInMonthResponse();

            result.DeviceNameList = query.Select(x => x.DEVICE_NAME).ToList();
            result.RepairList = query.Select(x => x.RepairCount).ToList();
            result.HourList = query.Select(x => x.TotalHours).ToList();


            return result;

        }


        public async Task<GetConstructionEchartDataResponse> GetConstructionEchartData()
        {
            /*
               淡水消耗 
                      按月统计出所有日耗  DAILYCONSUMPTION
               柴油消耗
                     按月统计出所有小计 SUBTOTAL
               滑油 
                    按月统计出所有小计 LUBRICATE
             */
            GetConstructionEchartDataResponse result = new GetConstructionEchartDataResponse();

            var query = await _dbContext.Query<BUILD_COUNT>()
                                  .Select(x => new
                                  {
                                      x.STARTDATE,
                                      //x.REPAIRTIME,
                                      //x.WEATHEREFFECT,
                                      //x.OTHERSTOP,
                                      x.DAILYCONSUMPTION,
                                      x.SUBTOTAL,
                                      x.LUBRICATE
                                  }).GroupBy(g => new { g.STARTDATE.Month })
                                   .Select(x => new
                                   {
                                       Month = x.STARTDATE.Month,
                                       //REPAIRTIME = Sql.Sum(x.REPAIRTIME),
                                       //WEATHEREFFECT = Sql.Sum(x.WEATHEREFFECT),
                                       //OTHERSTOP = Sql.Sum(x.OTHERSTOP),
                                       DAILYCONSUMPTION = Sql.Sum(x.DAILYCONSUMPTION),
                                       SUBTOTAL = Sql.Sum(x.SUBTOTAL),
                                       LUBRICATE = Sql.Sum(x.LUBRICATE)

                                   }).ToListAsync();
             result.FreshWaterCostList = Enumerable.Range(1, 12)
            .GroupJoin(query, m => m, q => q.Month, (m, q) => q.FirstOrDefault()?.DAILYCONSUMPTION ??0)
            .ToList();

            result.DieselOilCostList = Enumerable.Range(1, 12)
                .GroupJoin(query, m => m, q => q.Month, (m, q) => q.FirstOrDefault()?.SUBTOTAL ?? 0)
                .ToList();

            result.LubeCostList = Enumerable.Range(1, 12)
                .GroupJoin(query, m => m, q => q.Month, (m, q) => q.FirstOrDefault()?.LUBRICATE ?? 0)
                .ToList();

            return result;
        }

        #endregion

    }
}