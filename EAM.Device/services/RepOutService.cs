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

namespace EAM.Device.services
{
    public class RepOutService : IRepOutService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxService;
        private readonly UserSession _userSession;

        public RepOutService(IDbContext dbContext, IComboxDataService comboxService, UserSession userSession)
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
                { "MaintDept",null},
                { "RepSourceType",null},
                { "RepOutType",null},
                //{ "ProviderData",null},
            });
        }

        #region 委外维修确认

        /// <summary>
        /// 提交委外维修确认
        /// </summary>
        /// <returns></returns>
        public async Task<int> SubmitRepOutCheck(List<string> sids)
        {
            foreach (var sid in sids)
            {
                var qry = await _dbContext.Query<REP_OUT>()
                    .Where(c => c.OUT_ID == sid)
                    .Select(c => new
                    {
                        c.REP_DEVICE,
                        c.CONFIRM_USER,
                        c.OUT_REPAIR_MEMO,
                    }).FirstAsync();
                if (qry.REP_DEVICE == null || qry.CONFIRM_USER == null || qry.OUT_REPAIR_MEMO == null)
                {
                    throw new MessageException("核对委外维修确认单是否填写完成！");
                }
            }
            var updaterepout = await _dbContext.UpdateAsync<REP_OUT>(x => sids.Contains(x.OUT_ID),
                 x => new REP_OUT
                 {
                     OUT_STATUS = "25",
                     AUDITING = "1",
                 });
            return updaterepout;
        }

        /// <summary>
        /// 管理委外维修确认
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManageRepOut(SaveRequest<REP_OUT> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.REP_DEVICE,
                    c.CONFIRM_USER,
                    c.OUT_REPAIR_MEMO,
                    c.IS_LEAVE,
                    c.REASON,
                    c.SOLUTION,
                    c.LEAVE_MEMO,
                    c.MEMO,
                    c.OUT_ID,
                },
                c => a => a.OUT_ID == c.OUT_ID);
        }

        /// <summary>
        /// 获取委外维修确认列表
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetRepOutCheckList(GridRequest request)
        {
            return await _dbContext.Query<REP_OUT>()
                .LeftJoin<DEVICE_CARD>((a, b) => a.DEVICE_ID == b.DEVICE_ID)
                .Select((a, b) => new
                {
                    a.AUDITING,
                    a.OUT_STATUS,
                    a.OUT_CODE,
                    a.WDEPT_NAME,
                    a.REP_DEVICE,
                    a.REAL_ENDTIME,
                    a.REAL_BEGINTIME,
                    a.IS_LEAVE,
                    a.SRC_CODE,
                    a.WDEPT_ID,
                    a.DEPT_ID,
                    a.DEPT_NAME,
                    a.SRC_TYPE,
                    b.SITE_NAME,
                    b.DEVICE_NAME,
                    b.DEVICE_NO,
                    b.DEVICE_TYPE,
                    b.INSTALL_SITE,
                    a.OUT_ID,
                })
                .GetGridData(request);
        }

        /// <summary>
        /// 获取单条确认记录
        /// </summary>
        /// <returns></returns>

        public async Task<REP_OUT> GetRepOutDetail(string ID)
        {
            var qry = await _dbContext.QueryByKeyAsync<REP_OUT>(ID);
            return qry;
        }

        #endregion 委外维修确认

        #region 委外维修验收

        /// <summary>
        /// 提交委外维修验收
        /// </summary>
        /// <returns></returns>
        public async Task<int> SubmitRepOutAccept(List<string> sids)
        {
            foreach (var sid in sids)
            {
                var qry = await _dbContext.Query<REP_OUT>()
                    .Where(c => c.OUT_ID == sid)
                    .Select(c => new
                    {
                        c.CHECK_DESC,
                    }).FirstAsync();
                if (qry.CHECK_DESC == null)
                {
                    throw new MessageException("核对委外维修验收单是否填写完成！");
                }
            }
            var updaterepout = await _dbContext.UpdateAsync<REP_OUT>(x => sids.Contains(x.OUT_ID),
                x => new REP_OUT
                {
                    OUT_STATUS = "30",
                    AUDITING_CHK = "1",
                });
            return updaterepout;
        }

        /// <summary>
        /// 管理委外维修验收
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManageRepOutAccept(SaveRequest<REP_OUT> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.CHECK_DESC,
                    c.PACT_CODE,
                    c.PACT_NAME,
                    c.PACT_DATE,
                    c.REP_MONEY,
                    c.NOTAX_MONEY,
                    c.RATIO,
                    c.CHECK_USER,
                    c.CHECK_USERID,
                    c.PROVIDER_ID,
                    c.REAL_BEGINTIME,
                    c.REAL_ENDTIME,
                    c.ACT_STOP_TIME,
                    c.CHECK_DATE,
                    c.EIDT_DATE,
                    c.EIDT_USER,
                    c.OUT_ID,
                },
                c => a => a.OUT_ID == c.OUT_ID, null, null);
        }

        /// <summary>
        /// 获取委外维修验收列表
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetRepOutAcceptList(GridRequest request)
        {
            return await _dbContext.Query<REP_OUT>()
                .LeftJoin<DEVICE_CARD>((a, b) => a.DEVICE_ID == b.DEVICE_ID)
                .Select((a, b) => new
                {
                    a.AUDITING,
                    a.AUDITING_CHK,
                    a.OUT_STATUS,
                    a.OUT_CODE,
                    a.CHECK_USER,
                    a.CONFIRM_USER,
                    a.CONFIRM_USERID,
                    a.CHECK_USERID,
                    a.WDEPT_NAME,
                    a.WDEPT_ID,
                    a.REP_DEVICE,
                    a.REAL_ENDTIME,
                    a.REAL_BEGINTIME,
                    a.IS_LEAVE,
                    a.SRC_CODE,
                    a.DEPT_NAME,
                    a.SRC_TYPE,
                    b.SITE_NAME,
                    b.DEVICE_NAME,
                    b.DEVICE_NO,
                    b.DEVICE_TYPE,
                    b.INSTALL_SITE,
                    a.PACT_DATE,
                    a.PACT_CODE,
                    a.PACT_NAME,
                    a.EIDT_DATE,
                    a.EIDT_USER,
                    a.REP_MONEY,
                    a.NOTAX_MONEY,
                    a.RATIO,
                    a.PROVIDER_ID,
                    a.CHECK_DATE,
                    a.ACT_STOP_TIME,
                    a.CHECK_DESC,
                    a.OUT_ID,
                })
                .Where(c => c.AUDITING == "1")
                .GetGridData(request);
        }

        /// <summary>
        /// 获取委外维修验收列表
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetRepOutAcceptDetail(GridRequest request)
        {
            return await _dbContext.Query<REP_PLAN_SP>()
                .GetGridData(request);
        }

        #endregion 委外维修验收
    }
}