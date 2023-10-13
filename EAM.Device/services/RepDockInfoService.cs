using Chloe;
using DocumentFormat.OpenXml.Wordprocessing;
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
    public class RepDockInfoService : IRepDockInfoService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxService;
        private readonly UserSession _userSession;
        private DateTime? _Sysdate;

        /// <summary>
        /// 获取数据库时间
        /// </summary>
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
        public RepDockInfoService(IDbContext dbContext, IComboxDataService comboxService, UserSession userSession)
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
                { "DockInfo",null},
            });
        }

        /// <summary>
        /// 获取码头记录
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetBaseDockList(GridRequest request)
        {
            return await _dbContext.Query<BASE_DOCK>()
                .GetGridData(request);
        }

        #region 码头基础信息

        /// <summary>
        /// 获取单条码头记录
        /// </summary>
        /// <returns></returns>

        public async Task<BASE_DOCK> GetBaseDockListDetail(string ID)
        {
            var qry = await _dbContext.QueryByKeyAsync<BASE_DOCK>(ID);
            return qry;
        }

        /// <summary>
        /// 管理码头记录
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManageBaseDock(SaveRequest<BASE_DOCK> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.DOCK_CODE,
                    c.DOCK_NAME,
                    c.DOCK_ADDRESS,
                    c.EDIT_USER,
                    c.EDIT_USERID,
                    c.MEMO,
                    c.AUDITING,
                    c.EDIT_DATE,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.DOCK_ID,
                },
                c => a => a.DOCK_ID  == c.DOCK_ID, BeforeAdd);
        }

        public async Task BeforeAdd(BASE_DOCK entity)
        {
            entity.EDIT_USER = _userSession.RealName;
            entity.EDIT_USERID = _userSession.UserID.ToString();
            entity.EDIT_DATE = await _dbContext.GetSysdate();
            string aa = "MT" + DateTime.Now.ToString("yyyyMM");
            string def = aa + "0000";
            var model = await _dbContext.Query<BASE_DOCK>(x => x.DOCK_CODE.Contains(aa)).Select(x => Sql.Max(x.DOCK_CODE) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(8, 4).CastTo<int>() + 1;
            entity.DOCK_CODE = aa + index.ToString("D4");
            entity.AUDITING = "0";
            entity.DOCK_ID = GuidHelper.NewSnowflakeId().ToString();
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        public async Task<int> Submit(List<string> sids)
        {
            return await _dbContext.UpdateAsync<BASE_DOCK>(x => sids.Contains(x.DOCK_ID),
                x => new BASE_DOCK
                {
                    AUDITING = "1",
                });
        }

        /// <summary>
        /// 反提交
        /// </summary>
        /// <returns></returns>
        public async Task<int> UnSubmit(string sid)
        {
            return await _dbContext.UpdateAsync<BASE_DOCK>(x => sid == x.DOCK_ID,
                x => new BASE_DOCK
                {
                    AUDITING = "0",
                });

        }

        #endregion 码头基础信息

        #region 码头维修计划

        /// <summary>
        /// 获取维修计划记录
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetRepDockPlanList(GridRequest request)
        {
            return await _dbContext.Query<REP_DOCK_PLAN>()
                .OrderBy(c => c.AUDITING_PLAN)
                .ThenByDesc(c => c.EDIT_DATE)
                .GetGridData(request);
        }

        /// <summary>
        /// 获取单条维修计划记录
        /// </summary>
        /// <returns></returns>

        public async Task<REP_DOCK_PLAN> GetRepDockPlanListDetail(string ID)
        {
            var qry = await _dbContext.QueryByKeyAsync<REP_DOCK_PLAN>(ID);
            return qry;
        }

        /// <summary>
        /// 管理维修计划记录
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManageRepDockPlan(SaveRequest<REP_DOCK_PLAN> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING_PLAN,
                    c.PLAN_CODE,
                    c.DOCK_CODE,
                    c.DOCK_ID,
                    c.DOCK_NAME,
                    c.EDIT_USERID,
                    c.EDIT_USER,
                    c.EDIT_DATE,
                    c.REP_DESC,
                    c.MEMO,
                    c.PLAN_ID,
                    c.DEPT_ID,
                    c.DEPT_NAME
                },
                c => a => a.PLAN_ID == c.PLAN_ID, BeforeAdd);
        }

        public async Task BeforeAdd(REP_DOCK_PLAN entity)
        {
            entity.DEPT_ID = _userSession.Corp.CorpID;
            entity.DEPT_NAME = _userSession.Corp.CName;
            entity.EDIT_USER = _userSession.UserName;
            entity.EDIT_USERID = _userSession.UserID.ToString();
            entity.EDIT_DATE = Sysdate;
            string aa = "MTJH" + DateTime.Now.ToString("yyyyMM");
            string def = aa + "0000";
            var model = await _dbContext.Query<REP_DOCK_PLAN>(x => x.PLAN_CODE.Contains(aa)).Select(x => Sql.Max(x.PLAN_CODE) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(10, 4).CastTo<int>() + 1;
            entity.PLAN_CODE = aa + index.ToString("D4");
            entity.AUDITING_PLAN = "0";
            entity.PLAN_ID = GuidHelper.NewSnowflakeId().ToString();
        }

        /// <summary>
        /// 提交维修计划
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> SubmitRepDockPlan(List<string> sids)
        {
            string aa = "MTSS" + DateTime.Now.ToString("yyyyMM");
            string def = aa + "0000";
            var model = await _dbContext.Query<REP_DOCK_PLAN>(x => x.EXE_CODE.Contains(aa)).Select(x => Sql.Max(x.EXE_CODE) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(10, 4).CastTo<int>();
            foreach (var sid in sids)
            {
                index++;

                string newExeCode = aa + index.ToString("D4");

                await _dbContext.UpdateAsync<REP_DOCK_PLAN>(
                    x => x.PLAN_ID == sid,
                    x => new REP_DOCK_PLAN
                    {
                        AUDITING_PLAN = "1",
                        AUDITING_EXE = "0",
                        EXE_CODE = newExeCode,
                    });
            }
            return AjaxResult.Success("更新成功");
        }

        /// <summary>
        /// 反提交维修计划
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> UnSubmitRepDockPlan(string sid)
        {
            var qryplan =await _dbContext.Query<REP_DOCK_PLAN>(c => c.PLAN_ID == sid && c.AUDITING_EXE == "1").FirstOrDefaultAsync();
            if (qryplan == null) {
                await _dbContext.UpdateAsync<REP_DOCK_PLAN>(
                    x => x.PLAN_ID == sid,
                    x => new REP_DOCK_PLAN
                    {
                        AUDITING_PLAN = "0",
                        AUDITING_EXE = "",
                        EXE_CODE = "",
                    });
            }
            else
            {
                throw new MessageException("该计划已实施，不可反提交！");
            }
            return AjaxResult.Success("更新成功");
        }

        /// <summary>
        /// 获取计划明细
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetPlandetList(GridRequest request)
        {
            return await _dbContext.Query<REP_DOCK_PLAN_ITEM>()
                .GetGridData(request);
        }

        /// <summary>
        /// 管理计划明细
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManagePlandet(SaveRequest<REP_DOCK_PLAN_ITEM> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.REP_ITEM,
                    c.REP_CONTENT,
                    c.REP_METHOD,
                    c.LABOR_NUM,
                    c.TAKE_TIME,
                    c.REP_LEADER,
                    c.MEMO,
                    c.PLAN_ITEM_ID,
                    c.PLAN_ID,
                    c.IS_COMPLETE,
                    c.EXE_DATE,
                    c.EXE_LABOR_NUM,
                    c.EXE_TAKE_TIME,
                },
                c => a => a.PLAN_ITEM_ID == c.PLAN_ITEM_ID, BeforeAddPlandet, UpdateAddPlandet);
        }

        public async Task BeforeAddPlandet(REP_DOCK_PLAN_ITEM entity)
        {
            if (entity.LABOR_NUM<0||entity.TAKE_TIME<0) {
                throw new MessageException("预计人员数量或预计花费时间不可为负数！");
            }
            entity.PLAN_ITEM_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.IS_COMPLETE = "0";
            await Task.CompletedTask;
        }
        public async Task UpdateAddPlandet(REP_DOCK_PLAN_ITEM entity)
        {
            if (entity.LABOR_NUM<0||entity.TAKE_TIME<0)
            {
                throw new MessageException("预计人员数量或预计花费时间不可为负数！");
            }
            await Task.CompletedTask;
        }

        #endregion 码头维修计划

        #region 码头维修实施
        /// <summary>
        /// 提交维修实施
        /// </summary>
        /// <returns></returns>
        public async Task<int> SubmitRepDockExe(List<string> sids)
        {
            var querys = _dbContext.Query<REP_DOCK_PLAN>()
                 .Where(c => sids.Contains(c.PLAN_ID)).Select(c => new
                 {
                     c.IS_LEAVE,
                     c.EXE_EDATE,
                     c.EXE_BDATE,
                     c.EXE_CODE,
                     c.PLAN_ID,
                     c.REP_DESC,
                     c.EXE_COST,
                 }).ToList();
            foreach (var query in querys)
            {
                if (query.IS_LEAVE==null||query.EXE_EDATE==null||query.EXE_BDATE==null)
                {
                    throw new MessageException("码头实施详情没填完整！");
                }
                var qrydets = _dbContext.Query<REP_DOCK_PLAN_ITEM>()
                 .Where(c => c.PLAN_ID == query.PLAN_ID)
                 .Select(c => new
                 {
                     c.REP_ITEM,
                     c.REP_CONTENT,
                     c.REP_METHOD,
                     c.LABOR_NUM,
                     c.TAKE_TIME,
                     c.REP_LEADER,
                     c.MEMO,
                     c.PLAN_ITEM_ID,
                     c.PLAN_ID,
                     c.IS_COMPLETE,
                     c.EXE_DATE,
                     c.EXE_LABOR_NUM,
                     c.EXE_TAKE_TIME,
                 }).ToList();
                var checkList = new List<REP_DOCK_CHECK>();
                foreach (var qrydet in qrydets)
                {
                    var plandet = new REP_DOCK_CHECK()
                    {
                        CHECK_ID = GuidHelper.NewSnowflakeId().ToString(),
                        AUDITING_CONFIRM = "0",
                        EXE_CODE = query.EXE_CODE,
                        FAULT_DESCRIBE = query.REP_DESC,
                        REP_CONTENT = qrydet.REP_CONTENT,
                        REP_ITEM = qrydet.REP_ITEM,
                        PLAN_ID = query.PLAN_ID,
                        EXE_BDATE = query.EXE_BDATE,
                        EXE_EDATE = query.EXE_EDATE,
                        IS_LEAVE = query.IS_LEAVE,
                        PLAN_ITEM_ID = qrydet.PLAN_ITEM_ID,
                        CREATE_USERID = _userSession.UserID.ToString(),
                        CREATEDATE = Sysdate,
                    };
                    checkList.Add(plandet);
                }
                await _dbContext.InsertRangeAsync(checkList);
            }
            return await _dbContext.UpdateAsync<REP_DOCK_PLAN>(x => sids.Contains(x.PLAN_ID),
                        x => new REP_DOCK_PLAN
                        {
                            AUDITING_EXE = "1",
                            EXE_DATE = Sysdate,
                        });

        }

        /// <summary>
        /// 反提交维修实施
        /// </summary>
        /// <returns></returns>
        public async Task<int> UnSubmitRepDockExe(string sid)
        {
            var qryexeitem = await _dbContext.Query<REP_DOCK_CHECK>(c => c.EXE_CODE == sid)
                .Select(c=>c.AUDITING_CONFIRM)
                .ToListAsync();
            if (qryexeitem.Contains("1"))
            {
                throw new MessageException("确认单已提交，实施不可反提交！");
            }
            else
            {
                await _dbContext.DeleteAsync<REP_DOCK_CHECK>(c => c.EXE_CODE == sid);
                return await _dbContext.UpdateAsync<REP_DOCK_PLAN>(
                   x => x.EXE_CODE == sid,
                   x => new REP_DOCK_PLAN
                   {
                       AUDITING_EXE = "0",
                       EXE_DATE = null,
                   });
            }
        }

        /// <summary>
        /// 管理维修实施结果
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManageRepDockExe(SaveRequest<REP_DOCK_PLAN> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.IS_LEAVE,
                    c.EXE_EDATE,
                    c.EXE_BDATE,
                    c.EXE_CODE,
                    c.PLAN_ID,
                    c.REP_DESC,
                    c.EXE_USER,
                    c.EXE_USERID,
                    c.MEMO,
                    c.EXE_HOUR,
                    c.EXE_COST,
                    c.LEAVE_DESCRIBE,
                    c.EXE_DESCRIBE,
                },
                c => a => a.PLAN_ID == c.PLAN_ID, null, BeforeUpdateExe);
        }

        public async Task BeforeUpdateExe(REP_DOCK_PLAN entity)
        {
            entity.EXE_DATE = Sysdate;
            await Task.CompletedTask;
        }

        /// <summary>
        /// 获取维修实施记录
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetRepDockExeList(GridRequest request)
        {
            return await _dbContext.Query<REP_DOCK_PLAN>()
                .Where(c => c.AUDITING_PLAN=="1")
                .OrderBy(c => c.AUDITING_EXE)
                .ThenByDesc(c => c.EXE_CODE)
                .GetGridData(request);
        }
        #endregion

        #region 码头维修确认

        /// <summary>
        /// 提交码头维修确认
        /// </summary>
        /// <returns></returns>
        public async Task<int> SubmitRepDockConfirm(List<string> sids)
        {
            foreach (var sid in sids)
            {
                var qry = await _dbContext.Query<REP_DOCK_CHECK>()
                    .Where(c => c.CHECK_ID==sid)
                    .Select(c => new
                    {
                        c.IS_LEAVE,
                        c.REP_CONTENT,
                    }).FirstAsync();
                if (qry.IS_LEAVE == null||qry.REP_CONTENT == null)
                {
                    throw new MessageException("核对码头维修确认单是否填写完成！");
                }
            }
            var updaterepout = await _dbContext.UpdateAsync<REP_DOCK_CHECK>(x => sids.Contains(x.CHECK_ID),
                 x => new REP_DOCK_CHECK
                 {
                     AUDITING_CHECK = "0",
                     AUDITING_CONFIRM = "1",
                 });
            return updaterepout;
        }

        /// <summary>
        /// 反提交码头维修确认
        /// </summary>
        /// <returns></returns>
        public async Task<int> UnSubmitRepDockConfirm(string sid)
        {
            var updaterepout = await _dbContext.UpdateAsync<REP_DOCK_CHECK>(x => sid==x.CHECK_ID,
                 x => new REP_DOCK_CHECK
                 {
                     AUDITING_CHECK = "",
                     AUDITING_CONFIRM = "0",
                 });
            return updaterepout;
        }

        /// <summary>
        /// 获取码头维修确认列表
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetRepDockConfirmList(GridRequest request)
        {
            return await _dbContext.Query<REP_DOCK_CHECK>()
                .OrderBy(c => c.AUDITING_CONFIRM)
                .ThenByDesc(c => c.CONFIRM_CODE)
                .GetGridData(request);
        }

        /// <summary>
        /// 获取单条确认记录
        /// </summary>
        /// <returns></returns>

        public async Task<REP_DOCK_CHECK> GetRepDockConfirmDetail(string ID)
        {
            var qry = await _dbContext.QueryByKeyAsync<REP_DOCK_CHECK>(ID);
            return qry;
        }

        #endregion 码头维修确认

        #region 码头维修验收

        /// <summary>
        /// 提交码头维修验收
        /// </summary>
        /// <returns></returns>
        public async Task<int> SubmitRepDockCheck(List<string> sids)
        {
            foreach (var sid in sids)
            {
                var qry = await _dbContext.Query<REP_DOCK_CHECK>()
                    .Where(c => c.CHECK_ID==sid)
                    .Select(c => new
                    {
                        c.CHECK_DESC,
                    }).FirstAsync();
                if (qry.CHECK_DESC == null)
                {
                    throw new MessageException("核对码头维修验收单是否填写完成！");
                }
            }
            var updaterepout = await _dbContext.UpdateAsync<REP_DOCK_CHECK>(x => sids.Contains(x.CHECK_ID),
                x => new REP_DOCK_CHECK
                {
                    AUDITING_CHECK = "1",
                });
            return updaterepout;
        }

        /// <summary>
        /// 反提交码头维修验收
        /// </summary>
        /// <returns></returns>
        public async Task<int> UnSubmitRepDockCheck(string sid)
        {
            var updaterepout = await _dbContext.UpdateAsync<REP_DOCK_CHECK>(x => sid.Contains(x.CHECK_ID),
                x => new REP_DOCK_CHECK
                {
                    AUDITING_CHECK = "0",
                });
            return updaterepout;
        }


        /// <summary>
        /// 管理码头维修验收，确认
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManageRepDockCheck(SaveRequest<REP_DOCK_CHECK> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING_CHECK,
                    c.REP_ITEM,
                    c.FAULT_DESCRIBE,
                    c.EXE_CODE,
                    c.CONFIRM_CODE,
                    c.CHECK_CODE,
                    c.REASON,
                    c.SOLUTION,
                    c.REP_CONTENT,
                    c.IS_LEAVE,
                    c.LEAVE_MEMO,
                    c.CONFIRM_USERID,
                    c.CONFIRM_USER,
                    c.MEMO,
                    c.PACT_CODE,
                    c.PACT_NAME,
                    c.PACT_MONEY,
                    c.PACT_DATE,
                    c.PROVIDER_NAME,
                    c.PROVIDER_ID,
                    c.REP_MONEY,
                    c.CHECK_USER,
                    c.CHECK_DATE,
                    c.CHECK_DESC,
                    c.CHECK_ID,
                },
                c => a => a.CHECK_ID == c.CHECK_ID);
        }

        /// <summary>
        /// 获取码头维修验收列表
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetRepDockCheckList(GridRequest request)
        {
            return await _dbContext.Query<REP_DOCK_CHECK>()
                .Where(c => c.AUDITING_CONFIRM=="1")
                .OrderBy(c => c.AUDITING_CHECK)
                .ThenByDesc(c => c.CHECK_CODE)
                .GetGridData(request);
        }

        #endregion 码头维修验收
    }
}