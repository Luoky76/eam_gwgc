using EAM.Repair.interfaces;
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
            result.exeList = await _dbContext.Query<REP_PLAN_EXE>().WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName)
                                               .Select(x => new todolist { ID = x.EXE_ID, TEXT = x.EXE_CODE, TYPENAME = "维修计划", MENUNAME = "exe" }).ToListAsync();

            result.check = await _dbContext.Query<REP_PLAN_EXE>(x => x.AUDITING == "1" && x.DEAL_TYPE == "自修").WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName).CountAsync();
            result.checkList = await _dbContext.Query<REP_PLAN_EXE>(x => x.AUDITING == "1" && x.DEAL_TYPE == "自修")
                                    .WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName)
                                    .Select(x => new todolist { ID = x.EXE_ID, TEXT = x.EXE_CODE, TYPENAME = "维修验收", MENUNAME = "check" }).ToListAsync();

            result.ExtMainteCheck = await _dbContext.Query<REP_OUT>(x => x.AUDITING == "1").WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName).CountAsync();
            result.ExtMainteList = await _dbContext.Query<REP_OUT>(x => x.AUDITING == "1").WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName)
                                   .Select(x => new todolist { ID = x.OUT_ID, TEXT = x.OUT_CODE, TYPENAME = "委外维修验收", MENUNAME = "ExtMainteCheck" }).ToListAsync();

            result.ExtCheck = await _dbContext.Query<REP_OUT>().WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName).CountAsync();
            result.ExtCheckList = await _dbContext.Query<REP_OUT>().WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName)
                                     .Select(x => new todolist { ID = x.OUT_ID, TEXT = x.OUT_CODE, TYPENAME = "委外维修确认", MENUNAME = "ExtCheck" }).ToListAsync();

            result.RepDockExe = await _dbContext.Query<REP_DOCK_PLAN>(x => x.AUDITING_PLAN == "1").WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName).CountAsync();
            result.RepDockExeList = await _dbContext.Query<REP_DOCK_PLAN>(x => x.AUDITING_PLAN == "1").WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName)
                       .Select(x => new todolist { ID = x.PLAN_ID, TEXT = x.EXE_CODE, TYPENAME = "码头维修实施", MENUNAME = "RepDockExe" }).ToListAsync();

            result.RepDockConfirm = await _dbContext.Query<REP_OUT>().WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName).CountAsync();
            //维保实施   .WhereIf(!_userSession.IsAdmin, a => _userSession.ParentCompany.CorpID == a.SEC_DEPTID)
            result.RepDockConfirmList = await _dbContext.Query<REP_OUT>().WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName)
           .Select(x => new todolist { ID = x.OUT_ID, TEXT = x.OUT_CODE, TYPENAME = "码头维修确认", MENUNAME = "RepDockConfirm" }).ToListAsync();

            result.PmPlanExe = await _dbContext.Query<PM_PLAN_EXE>(x => x.AUDITING == "1").WhereIf(1 != 1, x => x.DEPT_NAME == _user.ParentCompany.CName).CountAsync();
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

    }
}