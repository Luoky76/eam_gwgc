using Chloe;
using EAM.Device.interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace EAM.Device.services
{
    public class DeviceStopService : IDeviceStopService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxService;
        private readonly UserSession _userSession;

        public DeviceStopService(IDbContext dbContext, IComboxDataService comboxService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxService = comboxService;
            _userSession = userSession;
        }

        /// <summary>
        /// 下拉
        /// </summary>
        /// <returns></returns>
        public async Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData()
        {
            return await _comboxService.Get(new Dictionary<string, object>(){
                { "StopSource", null },
                { "MalType", null },
                { "RepType", null },
                { "DeviceInfo", (Expression<Func<DEVICE_CARD, bool>>)(a => a.TYPE_ID == "1") },
            });
        }

        /// <summary>
        /// 获取停机记录
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetStopList(GridRequest request)
        {
            return await _dbContext.Query<RUN_STOP>()
                .OrderBy(c => c.AUDITING)
                .ThenByDesc(c => c.RUN_START)
                .GetGridData(request);
        }

        /// <summary>
        /// 获取单条停机记录
        /// </summary>
        /// <returns></returns>

        public async Task<RUN_STOP> GetStopListDetail(string ID)
        {
            var qry = await _dbContext.QueryByKeyAsync<RUN_STOP>(ID);
            return qry;
        }

        /// <summary>
        /// 管理停机记录
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManageStop(SaveRequest<RUN_STOP> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING,
                    c.STOP_CODE,
                    c.DEVICE_ID,
                    c.DEVICE_NAME,
                    c.DEVICE_TYPE,
                    c.TYPE_NAME,
                    c.STOP_SOURCE,
                    c.RUN_START,
                    c.RUN_END,
                    c.STOP_HOURS,
                    c.EDIT_USER,
                    c.DEPT_NAME,
                    c.SEC_DEPT,
                    c.EDIT_DATE,
                    c.STOP_DESC,
                    c.CHECK_USER,
                    c.CHECK_USERID,
                    c.CHECK_DEPT,
                    c.CHECK_DATE,
                    c.MAL_TYPE_NAME,
                    c.MAL_PARTS,
                    c.CHECK_DESC,
                    c.MEMO,
                    c.DEPT_ID,
                    c.SEC_DEPTID,
                    c.EDIT_USERID,
                    c.APPLY_DEPT,
                    c.APPLY_DEPTID,
                    c.TYPE_ID,
                    c.PROBLEM_ID,
                    c.REPORT_ID,
                    c.MAL_TYPE_ID,
                    c.CHECK_DEPTID,
                    c.RUN_STOP_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                },
                c => a => a.RUN_STOP_ID == c.RUN_STOP_ID, BeforeAdd);
        }

        public async Task BeforeAdd(RUN_STOP entity)
        {
            entity.SEC_DEPTID = _userSession.ParentCompany.CorpID;
            entity.SEC_DEPT = _userSession.ParentCompany.CName;
            entity.DEPT_ID = _userSession.Corp.CorpID;
            entity.DEPT_NAME = _userSession.Corp.CName;
            entity.EDIT_USER = _userSession.RealName;
            entity.EDIT_DATE = await _dbContext.GetSysdate();
            string aa = "TG" + DateTime.Now.ToString("yyyyMM");
            string def = aa + "0000";
            var model = await _dbContext.Query<RUN_STOP>(x => x.STOP_CODE.Contains(aa)).Select(x => Sql.Max(x.STOP_CODE) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(8, 4).CastTo<int>() + 1;
            entity.STOP_CODE = aa + index.ToString("D4");
            entity.AUDITING = "0";
            entity.RUN_STOP_ID = GuidHelper.NewSnowflakeId().ToString();
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        public async Task<int> Submit(List<string> sids)
        {
            return await _dbContext.UpdateAsync<RUN_STOP>(x => sids.Contains(x.RUN_STOP_ID),
                x => new RUN_STOP
                {
                    AUDITING = "1",
                });
        }

        /// <summary>
        /// 获取停机分类
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetStopTypeList(GridRequest request)
        {
            return await _dbContext.Query<RUN_STOP_TYPE>()
                .GetGridData(request);
        }

        /// <summary>
        /// 管理停机分类
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManageStopType(SaveRequest<RUN_STOP_TYPE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.MEMO,
                    c.IS_STOP,
                    c.IS_PLAN,
                    c.STOP_NAME,
                    c.STOP_TYPE_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.TENANT_ID,
                },
                c => a => a.STOP_TYPE_ID == c.STOP_TYPE_ID, BeforeAdd);
        }

        public async Task BeforeAdd(RUN_STOP_TYPE entity)
        {
            entity.STOP_TYPE_ID = GuidHelper.NewSnowflakeId().ToString();
            await Task.CompletedTask;
        }
    }
}