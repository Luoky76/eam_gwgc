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
        private string allDataShow = "船机部";


        public WelcomeService(IDbContext dbContext, UserSession userSession)
        {
            _dbContext = dbContext;
            _user = userSession;
        }
        #region 待办数据处理
        public async Task<GetTodoListDataCountResponse> GetTodoListData()
        {
            GetTodoListDataCountResponse result = new GetTodoListDataCountResponse();
            result.exe = await _dbContext.Query<REP_PLAN_EXE>().WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName).CountAsync();
            result.exeTitle = "维修计划";
            result.exeList = await _dbContext.Query<REP_PLAN_EXE>().WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName)
                                               .Select(x => new todolist { ID = x.EXE_ID, TEXT = x.EXE_CODE, TYPENAME = "维修计划", MENUNAME = "exe" }).ToListAsync();

            result.check = await _dbContext.Query<REP_PLAN_EXE>(x => x.AUDITING == "1" && x.DEAL_TYPE == "自修").WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName).CountAsync();
            result.checkTitle = "维修验收";
            result.checkList = await _dbContext.Query<REP_PLAN_EXE>(x => x.AUDITING == "1" && x.DEAL_TYPE == "自修")
                                    .WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName)
                                    .Select(x => new todolist { ID = x.EXE_ID, TEXT = x.EXE_CODE, TYPENAME = "维修验收", MENUNAME = "check" }).ToListAsync();

            result.ExtMainteCheck = await _dbContext.Query<REP_OUT>(x => x.AUDITING == "1").WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName).CountAsync();
            result.ExtMainteCheckTitle = "委外维修验收";
            result.ExtMainteList = await _dbContext.Query<REP_OUT>(x => x.AUDITING == "1").WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName)
                                   .Select(x => new todolist { ID = x.OUT_ID, TEXT = x.OUT_CODE, TYPENAME = "委外维修验收", MENUNAME = "ExtMainteCheck" }).ToListAsync();

            result.ExtCheck = await _dbContext.Query<REP_OUT>().WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName).CountAsync();
            result.ExtCheckTitle = "委外维修确认";
            result.ExtCheckList = await _dbContext.Query<REP_OUT>().WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName)
                                     .Select(x => new todolist { ID = x.OUT_ID, TEXT = x.OUT_CODE, TYPENAME = "委外维修确认", MENUNAME = "ExtCheck" }).ToListAsync();

            result.RepDockExe = await _dbContext.Query<REP_DOCK_PLAN>(x => x.AUDITING_PLAN == "1").WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName).CountAsync();
            result.RepDockExeTitle = "码头维修实施";
            result.RepDockExeList = await _dbContext.Query<REP_DOCK_PLAN>(x => x.AUDITING_PLAN == "1").WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName)
                       .Select(x => new todolist { ID = x.PLAN_ID, TEXT = x.EXE_CODE, TYPENAME = "码头维修实施", MENUNAME = "RepDockExe" }).ToListAsync();

            result.RepDockConfirm = await _dbContext.Query<REP_OUT>().WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName).CountAsync();
            result.RepDockConfirmTitle = "码头维修确认";
            //维保实施   .WhereIf(!_userSession.IsAdmin, a => _userSession.ParentCompany.CorpID == a.SEC_DEPTID)
            result.RepDockConfirmList = await _dbContext.Query<REP_OUT>().WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName)
           .Select(x => new todolist { ID = x.OUT_ID, TEXT = x.OUT_CODE, TYPENAME = "码头维修确认", MENUNAME = "RepDockConfirm" }).ToListAsync();

            result.PmPlanExe = await _dbContext.Query<PM_PLAN_EXE>(x => x.AUDITING == "1").WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName).CountAsync();
            result.PmPlanExeTitle = "维保实施";
            result.PmPlanExeList = await _dbContext.Query<PM_PLAN_EXE>(x => x.AUDITING == "1").WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName)
                       .Select(x => new todolist { ID = x.EXE_ID, TEXT = x.EXE_CODE, TYPENAME = "维保实施", MENUNAME = "PmPlanExe" }).ToListAsync();


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

            //DateTime startTime = new DateTime(datetime.Year, datetime.Month, 1);
            GetDeviceRepairCountResponse result = new GetDeviceRepairCountResponse();
            result.deviceCount = await _dbContext.Query<DEVICE_CARD>(x => x.TYPE_NAME == "船舶" && x.AUDITING == "0").CountAsync();
            result.repairCount = await _dbContext.Query<REP_PLAN>(x => x.DEAL_TYPE == "自修" && x.AUDITING == "0").CountAsync();
            result.outrepairCount = await _dbContext.Query<REP_PLAN>(x => x.DEAL_TYPE == "外协" && x.AUDITING == "0").CountAsync();
            result.shiprepairCount = await _dbContext.Query<REP_DOCK_PLAN>(x => x.AUDITING_PLAN == "0").CountAsync();

            return result;
        }


        #endregion

        #region echart相关接口
        public async Task<GetDeviceRepairInfoEchartResponse> GetDeviceRepairInfoEchart()
        {
            //4.1  取出所有设备的维修次数和时长 按月分组， 除码头维修以外
            //                       4.2.点击设备维修情况（月） 的柱状图 要获取对应月份 取出当前月份所有维修设备的次数和维修时长 按设备分组
            //                        DEVICE_NAME
            //                       数据来源 ： 维修计划实施 REP_PLAN_EXE ACT_END_DATE 和故障处理 REP_FAULT   COMPLETE_DATE  并且是已提交的数据
            //                                  月份以实际完工时间为准
            DateTime nowTime =  _dbContext.GetSysdate().Result().Value;
            DateTime startTime = new DateTime(nowTime.Year, 1, 1);
            DateTime EndTime = new DateTime(nowTime.Year, 12, 31);



            var query = await _dbContext.Query<REP_PLAN_EXE>(x=>x.AUDITING=="1" &&    x.ACT_END_DATE.HasValue)
                .LeftJoin<DEVICE_CARD>((a,b)=>a.DEVICE_ID==b.DEVICE_ID)
                .Select((a,b)=> new { 
                     b.DEVICE_NAME,
                     a.ACT_END_DATE,
                     a.ACT_STOP_TIME,
                     a.DEVICE_ID,
                     a.EXE_ID
                })
                .GroupBy(e => new {  e.DEVICE_ID,e.DEVICE_NAME,e.ACT_END_DATE.Value })
                .Select(g => new
                {
           
                    Month = g.ACT_END_DATE.Value.Month,
                    g.DEVICE_NAME,
                    g.DEVICE_ID,
                    RepairCount = Sql.Count(g.EXE_ID),
                    TotalHours = (Sql.Sum(g.ACT_STOP_TIME)??(decimal)0.00 )/ (decimal) 60.00 // 转换为小时
                }).ToListAsync();

            var query2 = await _dbContext.Query<REP_FAULT>(x => x.AUDITING == "1" && x.COMPLETE_DATE.HasValue)
                .LeftJoin<DEVICE_CARD>((a, b) => a.DEVICE_ID == b.DEVICE_ID)
                .Select((a, b) => new {
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
                .Select(x => new {
                    x.Key.Month,
                    RepairCount =x.Sum(x=>x.RepairCount),
                    TotalHours = x.Sum(x=>x.TotalHours)  
                }).ToList();

            List<decimal> hourList = new List<decimal>();
            List<int> RepairList = new List<int>(); 
            for (int i = 1; i <= 12; i++)
            {
                var info = sumList.Where(x=>x.Month==i).FirstOrDefault();
                hourList.Add(info!=null?info.TotalHours : 0);
                RepairList.Add(info != null ? info.RepairCount : 0);
            }
            GetDeviceRepairInfoEchartResponse result =new GetDeviceRepairInfoEchartResponse ();
            result.hourList = hourList;
            result.RepairList = RepairList;
            return result;

        }

        public async Task<string> GetDeviceInfoInMonth(int month)
        {
            var query = await _dbContext.Query<REP_PLAN_EXE>(x => x.AUDITING == "1" && x.ACT_END_DATE.HasValue)
                                        .LeftJoin<DEVICE_CARD>((a, b) => a.DEVICE_ID == b.DEVICE_ID)
                                        .Select((a, b) => new {
                                            b.DEVICE_NAME,
                                            a.ACT_STOP_TIME,
                                            a.DEVICE_ID,
                                            a.EXE_ID
                                        })
                                        .GroupBy(e => new { e.DEVICE_ID, e.DEVICE_NAME})
                                        .Select(g => new
                                        {
                                            g.DEVICE_NAME,
                                            g.DEVICE_ID,
                                            RepairCount = Sql.Count(g.EXE_ID),
                                            TotalHours = (Sql.Sum(g.ACT_STOP_TIME) ?? (decimal)0.00) / (decimal)60.00 // 转换为小时
                                        }).ToListAsync();
          

            return "";

        }

        #endregion

    }
}