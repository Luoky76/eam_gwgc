using Chloe;
using EAM.Device.interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace EAM.Device.services
{
    public class PmInfoService : IPmInfoService
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

        public PmInfoService(IDbContext dbContext, IComboxDataService comboxService, UserSession userSession)
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
                { "PmType",null},
                { "BySource",null},
                { "MaintDept",null},
                { "WorkState",null},
                { "MaintCycle",null},
                { "BCCode", "store_src" },
                { "DeviceInfo",(Expression<Func<DEVICE_CARD, bool>>)(c => (c.TYPE_ID == "1"))},
            });
        }

        /// <summary>
        /// 同时保存维保计划、维保实施的所有主子表
        /// </summary>
        /// <param name="request1">维保计划</param>
        /// <param name="request2">维保项目明细</param>
        /// <param name="request3">物资明细</param>
        /// <param name="request4">人员明细</param>
        /// <param name="request5">特殊作业明细</param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAllAsync
            (SaveRequest<PM_PLAN_EXE> request1, SaveRequest<PM_PLAN_DONEITEM> request2, SaveRequest<PM_PLAN_SP> request3, SaveRequest<PM_PLAN_LABOR> request4, SaveRequest<PM_SPECIAL_WORK> request5)
        {
            string exe_id;
            //填写主子表关联键值
            if (request1.Updated.Any() && !request1.Updated.First().EXE_ID.IsNullOrWhiteSpace())
            {
                exe_id = request1.Updated.First().EXE_ID;
            }
            else if (request1.Added.Any() && !request1.Added.First().EXE_ID.IsNullOrWhiteSpace())
            {
                exe_id = request1.Added.First().EXE_ID;
            }
            else exe_id = GuidHelper.NewSnowflakeId().ToString();
            if (request1.Added.Any() && request1.Added.First().EXE_ID.IsNullOrWhiteSpace())
            {
                request1.Added[0].EXE_ID = exe_id;
            }

            foreach (var entity in request2.Added ??= new List<PM_PLAN_DONEITEM>())
            {
                if (entity.EXE_ID.IsNullOrWhiteSpace()) entity.EXE_ID = exe_id;
            }
            foreach (var entity in request3.Added ??= new List<PM_PLAN_SP>())
            {
                if (entity.EXE_ID.IsNullOrWhiteSpace()) entity.EXE_ID = exe_id;
            }
            foreach (var entity in request4.Added ??= new List<PM_PLAN_LABOR>())
            {
                if (entity.EXE_ID.IsNullOrWhiteSpace()) entity.EXE_ID = exe_id;
            }
            foreach (var entity in request5.Added ??= new List<PM_SPECIAL_WORK>())
            {
                if (entity.EXE_ID.IsNullOrWhiteSpace()) entity.EXE_ID = exe_id;
            }

            //启用事务保存所有表
            try
            {
                await _dbContext.UseTransactionAsync(async () =>
                {
                    if ((await ManagePmPlan(request1)).IsError)
                    {
                        throw new MessageException("维保计划保存失败");
                    }
                    if ((await ManagePlandet(request2)).IsError)
                    {
                        throw new MessageException("维保项目明细保存失败");
                    }
                    if ((await ManagePmSp(request3)).IsError)
                    {
                        throw new MessageException("物资明细保存失败");
                    }
                    if ((await ManagePmPep(request4)).IsError)
                    {
                        throw new MessageException("人员明细保存失败");
                    }
                    if ((await ManageWork(request5)).IsError)
                    {
                        throw new MessageException("特殊作业明细保存失败");
                    }
                });
            }
            catch (Exception ex)
            {
                return AjaxResult.Error(ex.Message);
            }
            return AjaxResult.Success("保存成功");
        }

        #region 维保计划

        /// <summary>
        /// 导入功能
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> ImportList(GridRequest request)
        {
            return await _dbContext.Query<PM_STD_LIST>()
                .GetGridData(request);
        }

        /// <summary>
        /// 获取维保计划记录
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetPmPlanList(GridRequest request)
        {
            //从 BC_CODE 取船机部的部门 ID
            var engineCorpId = (await _dbContext.Query<BC_CODE>(a => a.CODE_TYPE == "engineCorpId")
                .FirstAsync()).CODE_EN;
            //除超管和船机部外，按部门过滤数据
            return await _dbContext.Query<PM_PLAN_EXE>()
                .WhereIf(!_userSession.IsAdmin && _userSession.Corp.CorpID != engineCorpId, a => _userSession.Corp.CorpID == a.DEPT_ID)
                .Where(c => c.PM_TYPE == "20")
                .OrderBy(c => c.AUDITING)
                .ThenByDesc(c => c.PLAN_CODE)
                .GetGridData(request);
        }

        /// <summary>
        /// 获取单条维保计划记录
        /// </summary>
        /// <returns></returns>

        public async Task<PM_PLAN_EXE> GetPmPlanListDetail(string ID)
        {
            var qry = await _dbContext.QueryByKeyAsync<PM_PLAN_EXE>(ID);
            return qry;
        }

        /// <summary>
        /// 管理维保计划记录
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManagePmPlan(SaveRequest<PM_PLAN_EXE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING,
                    c.PLAN_CODE,
                    c.SOURCE,
                    c.DEVICE_NAME,
                    c.DEVICE_ID,
                    c.DEVICE_CODE,
                    c.BEGIN_DATE,
                    c.END_DATE,
                    c.MAINTENANCE_HOUR,
                    c.ASSET_CODE,
                    c.INSTALL_SITE,
                    c.DEPT_NAME,
                    c.PM_TYPE,
                    c.WDEPT_ID,
                    c.EDIT_DATE,
                    c.PLAN_FINISH_TIME,
                    c.EDIT_USER,
                    c.SHIP_DEPT,
                    c.EXE_USER,
                    c.EXE_USERID,
                    c.MEMO,
                    c.EXE_ID,
                    c.IS_LOSE,
                    c.LEG_DESC,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                },
                c => a => a.EXE_ID == c.EXE_ID, BeforeAdd);
        }

        public async Task BeforeAdd(PM_PLAN_EXE entity)
        {
            if (entity.EXE_ID.IsNullOrWhiteSpace())
            {
                entity.EXE_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            entity.SEC_DEPTID = _userSession.ParentCompany.CorpID;
            entity.SEC_DEPT = _userSession.ParentCompany.CName;
            entity.DEPT_ID = _userSession.Corp.CorpID;
            entity.DEPT_NAME = _userSession.Corp.CName;
            entity.EDIT_USER = _userSession.UserName;
            entity.EDIT_USERID = _userSession.UserID.ToString();
            entity.EDIT_DATE = Sysdate;
            string aa = "BYJH" + DateTime.Now.ToString("yyyyMM");
            string def = aa + "0000";
            var model = await _dbContext.Query<PM_PLAN_EXE>(x => x.PLAN_CODE.Contains(aa)).Select(x => Sql.Max(x.PLAN_CODE) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(10, 4).CastTo<int>() + 1;
            entity.PLAN_CODE = aa + index.ToString("D4");
            entity.AUDITING = "0";
            entity.AUDITING_EXE = "0";
        }

        /// <summary>
        /// 提交维保计划
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> SubmitPmPlan(List<string> sids)
        {
            //判断是否有明细，无明细的维保计划不允许提交
            foreach (var sid in sids)
            {
                var list = await _dbContext.Query<PM_PLAN_DONEITEM>(a => a.EXE_ID == sid)
                    .ToListAsync();
                if (!list.Any()) throw new MessageException("维保计划无明细数据，请添加维保项目！");
            }

            string aa = "BYSS" + DateTime.Now.ToString("yyyyMM");
            string def = aa + "0000";

            var model = await _dbContext.Query<PM_PLAN_EXE>(x => x.EXE_CODE.Contains(aa)).Select(x => Sql.Max(x.EXE_CODE) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(10, 4).CastTo<int>();

            foreach (var sid in sids)
            {
                index++;

                string newExeCode = aa + index.ToString("D4");

                await _dbContext.UpdateAsync<PM_PLAN_EXE>(
                    x => x.EXE_ID == sid,
                    x => new PM_PLAN_EXE
                    {
                        AUDITING = "1",
                        AUDITING_EXE = "0",
                        EXE_CODE = newExeCode,
                        EDIT_USER = _userSession.UserName,
                        EDIT_USERID = _userSession.UserID.ToString(),
                        EDIT_DATE = Sysdate
                    });
            }
            return AjaxResult.Success("更新成功");
        }

        /// <summary>
        /// 反提交维保计划
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> UnSubmitPmPlan(string sid)
        {
            var qryexe = await _dbContext.Query<PM_PLAN_EXE>(x => sid == x.EXE_ID)
                .Select(c => c.AUDITING_EXE)
                .FirstOrDefaultAsync();
            if (qryexe == "1")
            {
                throw new MessageException("实施已提交，计划不可撤销提交！");
            }
            else
            {
                await _dbContext.UpdateAsync<PM_PLAN_EXE>(
                    x => x.EXE_ID == sid,
                    x => new PM_PLAN_EXE
                    {
                        AUDITING = "0",
                        AUDITING_EXE = "0",
                        EXE_CODE = "",
                    });
            }
            return AjaxResult.Success("撤回提交成功");
        }

        /// <summary>
        /// 查询附件
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<AjaxResult> GetPmFileList(string Id)
        {
            var query = await _dbContext.Query<PM_PLAN_DONEITEM>(c => c.DONEITEM_ID.ToString() == Id).ToListAsync();

            var list = query.Select(c => new
            {
                ATTACH_EXE_FILE = string.Join(",", _dbContext.Query<SYS_ATTACH>().Where(a => a.data_id == c.DONEITEM_ID.ToString() && a.table_name == "PM_PLAN_EXE").Select(a => a.attach_path).ToList()),
                ATTACH_PLAN_FILE = string.Join(",", _dbContext.Query<SYS_ATTACH>().Where(a => a.data_id == c.DONEITEM_ID.ToString() && a.table_name == "PM_PLAN_DONEITEM").Select(a => a.attach_path).ToList())
            }).ToList();

            return AjaxResult.Success(list);
        }

        /// <summary>
        /// 获取计划明细
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetPlandetList(GridRequest request)
        {
            return await _dbContext.Query<PM_PLAN_DONEITEM>()
                 .Select(c => new
                 {
                     c.STD_CODE,
                     c.OBJECT_NAME,
                     c.CONTENT,
                     c.STD_LEVEL,
                     c.WORK_STATE,
                     c.MAINT_CYCLE,
                     c.PLAN_MONTH,
                     c.EXE_USER,
                     c.EXECUTE_USER,
                     c.CHK_USER,
                     c.CHECK_USER,
                     c.MEMO,
                     c.DONEITEM_ID,
                     c.EXE_ID,
                     c.COMPLETE,
                     ATTACH_EXE = _dbContext.Query<SYS_ATTACH>().Where(a => a.data_id == c.DONEITEM_ID.ToString() && a.table_name == "PM_PLAN_EXE").Count(),
                     ATTACH_PLAN = _dbContext.Query<SYS_ATTACH>().Where(a => a.data_id == c.DONEITEM_ID.ToString() && a.table_name == "PM_PLAN_DONEITEM").Count()
                 }).GetGridData(request);
        }

        /// <summary>
        /// 获取计划主表和明细信息
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetExtendPlanList(GridRequest request)
        {
            return await _dbContext.Query<PM_PLAN_EXE>()
                .LeftJoin<PM_PLAN_DONEITEM>((a, b) => a.EXE_ID == b.EXE_ID)
                .Select((a, b) => new
                {
                    a.AUDITING,
                    a.AUDITING_EXE,
                    a.EXE_ID,
                    a.DEPT_NAME,
                    a.SHIP_DEPT,
                    a.PLAN_FINISH_TIME,
                    a.AUDIT_TIME,
                    a.PLAN_CODE,
                    a.BEGIN_DATE,
                    a.END_DATE,
                    a.MAINTENANCE_HOUR,
                    b.STD_CODE,
                    b.OBJECT_NAME,
                    b.CONTENT,
                    b.STD_LEVEL,
                    b.WORK_STATE,
                    b.MAINT_CYCLE,
                    b.PLAN_MONTH,
                    b.EXE_USER,
                    b.EXECUTE_USER,
                    b.CHK_USER,
                    b.CHECK_USER,
                    b.MEMO,
                    b.DONEITEM_ID,
                    b.COMPLETE,
                    ATTACH_EXE = _dbContext.Query<SYS_ATTACH>().Where(c => c.data_id == b.DONEITEM_ID.ToString() && c.table_name == "PM_PLAN_EXE").Count(),
                    ATTACH_PLAN = _dbContext.Query<SYS_ATTACH>().Where(c => c.data_id == b.DONEITEM_ID.ToString() && c.table_name == "PM_PLAN_DONEITEM").Count()
                })
                .GetGridData(request);
        }

        /// <summary>
        /// 管理计划明细
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManagePlandet(SaveRequest<PM_PLAN_DONEITEM> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.MEMO,
                    c.STD_CODE,
                    c.EXE_USER,
                    c.CHK_USER,
                    c.OBJECT_NAME,
                    c.CONTENT,
                    c.STD_LEVEL,
                    c.WORK_STATE,
                    c.MAINT_CYCLE,
                    c.PLAN_MONTH,
                    c.CYCLE,
                    c.LAST_COMP_DATE,
                    c.NEXT_ENDDATE,
                    c.EXECUTE_USER,
                    c.CHECK_USER,
                    c.DONEITEM_ID,
                    c.COMPLETE,
                    c.EXE_ID,
                },
                c => a => a.DONEITEM_ID == c.DONEITEM_ID, BeforeAddPlandet);
        }

        public async Task BeforeAddPlandet(PM_PLAN_DONEITEM entity)
        {
            if (entity.DONEITEM_ID.IsNullOrWhiteSpace())
            {
                entity.DONEITEM_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            if (entity.EXE_ID.IsNullOrWhiteSpace())
            {
                throw new MessageException("外键 EXE_ID 为空！");
            }
            await Task.CompletedTask;
        }

        #endregion 维保计划

        #region 维保实施
        /// <summary>
        /// 导入物资功能
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> ImportSpList(GridRequest request)
        {
            return await _dbContext.Query<SP_STORE>(c => c.NUM > 0)
                .GetGridData(request);
        }
        /// <summary>
        /// 获取维保人员明细
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetPmPepList(GridRequest request, string exeId, string doneitemId)
        {
            return await _dbContext.Query<PM_PLAN_LABOR>(c => c.EXE_ID.Equals(exeId) && c.DONEITEM_ID.Equals(doneitemId))
                .GetGridData(request);
        }

        /// <summary>
        /// 获取维保物资明细
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetPmSpList(GridRequest request, string exeId, string doneitemId)
        {
            var query = _dbContext.Query<PM_PLAN_SP>(c => c.EXE_ID.Equals(exeId));
            if (!string.IsNullOrEmpty(doneitemId))
            {
                query = query.Where(c => c.DONEITEM_ID.Equals(doneitemId));
            }
            return await query.GetGridData(request);
        }

        /// <summary>
        /// 获取作业清单明细
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetWorkList(GridRequest request)
        {
            return await _dbContext.Query<PM_SPECIAL_WORK>()
                .GetGridData(request);
        }

        /// <summary>
        /// 管理作业清单明细
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManageWork(SaveRequest<PM_SPECIAL_WORK> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.MEMO,
                    c.WORK_NAME,
                    c.WORK_DATE,
                    c.BEGIN_TIME,
                    c.END_TIME,
                    c.WORK_HOUR,
                    c.WORK_OIL,
                    c.WORK_ID,
                    c.EXE_ID,
                },
                c => a => a.WORK_ID == c.WORK_ID, BeforeAddWork);
        }

        public async Task BeforeAddWork(PM_SPECIAL_WORK entity)
        {
            if (entity.WORK_ID.IsNullOrWhiteSpace())
            {
                entity.WORK_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            if (entity.EXE_ID.IsNullOrWhiteSpace())
            {
                throw new MessageException("外键 EXE_ID 为空！");
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 管理人员明细
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManagePmPep(SaveRequest<PM_PLAN_LABOR> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.MEMO,
                    c.USER_NAME,
                    c.PLAN_DATE,
                    c.USER_ID,
                    c.EXE_ID,
                    c.DONEITEM_ID,
                    c.PLAN_LABOR_ID,
                },
                c => a => a.PLAN_LABOR_ID == c.PLAN_LABOR_ID, BeforeAddLabor);
        }

        public async Task BeforeAddLabor(PM_PLAN_LABOR entity)
        {
            if (entity.PLAN_LABOR_ID.IsNullOrWhiteSpace())
            {
                entity.PLAN_LABOR_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            if (entity.EXE_ID.IsNullOrWhiteSpace())
            {
                throw new MessageException("外键 EXE_ID 为空！");
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 管理维保物资明细
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManagePmSp(SaveRequest<PM_PLAN_SP> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.SP_SOURCE,
                    c.SP_CODE,
                    c.SP_SIZE,
                    c.PRODUCE,
                    c.SP_NAME,
                    c.OTHER_CODE,
                    c.UNIT,
                    c.FACTORY,
                    c.HOUSE_NAME,
                    c.STORE_NUM,
                    c.FACT_NUM,
                    c.APPLY_NUM,
                    c.TAX_MONEY,
                    c.NOTAX_MONEY,
                    c.MEMO,
                    c.SP_HOUSE_ID,
                    c.HOUSE_ID,
                    c.SP_ID,
                    c.EXE_ID,
                    c.STOCK_NAME,
                    c.STOCK_ID,
                    c.DONEITEM_ID,
                    c.PLAN_SP_ID,
                },
                c => a => a.PLAN_SP_ID == c.PLAN_SP_ID, BeforeAddSp);
        }

        public async Task BeforeAddSp(PM_PLAN_SP entity)
        {
            if (entity.PLAN_SP_ID.IsNullOrWhiteSpace())
            {
                entity.PLAN_SP_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            if (entity.EXE_ID.IsNullOrWhiteSpace())
            {
                throw new MessageException("外键 EXE_ID 为空！");
            }
            await Task.CompletedTask;
        }


        /// <summary>
        /// 提交维保实施
        /// </summary>
        /// <returns></returns>
        public async Task<int> SubmitPmExe(List<string> sids)
        {
            var querys = _dbContext.Query<PM_PLAN_EXE>()
                 .Where(c => sids.Contains(c.EXE_ID)).Select(c =>
                 new
                 {
                     c.IS_LOSE,
                     c.EXE_ID
                 }).ToList();
            foreach (var query in querys)
            {
                if (query.IS_LOSE == null)
                {
                    throw new MessageException("是否有遗留问题没选！");
                }
                var qrydet = _dbContext.Query<PM_PLAN_DONEITEM>()
                 .Where(c => c.EXE_ID == query.EXE_ID && (c.CHECK_USER == null || c.EXECUTE_USER == null || c.COMPLETE == null))
                 .Select(c =>
                 new
                 {
                     c.CHECK_USER,
                     c.EXECUTE_USER,
                     c.COMPLETE,
                 }).ToList();
                if (qrydet.Count > 0)
                {
                    throw new MessageException("维保实施明细数据没填完整！");
                }
            }
            return await _dbContext.UpdateAsync<PM_PLAN_EXE>(x => sids.Contains(x.EXE_ID),
                        x => new PM_PLAN_EXE
                        {
                            AUDITING_EXE = "1",
                            AUDIT_TIME = Sysdate,
                        });

        }

        /// <summary>
        /// 反提交维保实施
        /// </summary>
        /// <returns></returns>
        public async Task<int> UnSubmitPmExe(string sid)
        {
            return await _dbContext.UpdateAsync<PM_PLAN_EXE>(x => sid == x.EXE_ID,
                        x => new PM_PLAN_EXE
                        {
                            AUDITING_EXE = "0",
                            AUDIT_TIME = null,
                        });

        }

        public async Task BeforeUpdateExe(PM_PLAN_EXE entity)
        {
            //entity.CHECK_USER = _userSession.UserName;
            //entity.CHECK_USERID = _userSession.UserID.ToString();
            //entity.CHECK_DATE = Sysdate;
            await Task.CompletedTask;
        }

        /// <summary>
        /// 获取维保实施记录
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetPmExeList(GridRequest request)
        {
            //从 BC_CODE 取船机部的部门 ID
            var engineCorpId = (await _dbContext.Query<BC_CODE>(a => a.CODE_TYPE == "engineCorpId")
                .FirstAsync()).CODE_EN;
            //除超管和船机部外，按部门过滤数据
            return await _dbContext.Query<PM_PLAN_EXE>()
                .WhereIf(!_userSession.IsAdmin && _userSession.Corp.CorpID != engineCorpId, a => _userSession.Corp.CorpID == a.DEPT_ID)
                .Where(c => c.AUDITING == "1")
                .OrderBy(c => c.AUDITING_EXE)
                .ThenByDesc(c => c.PLAN_CODE)
                .GetGridData(request);
        }

        #endregion 维保实施

        #region 维保实施查询

        /// <summary>
        /// 获取维保实施查询记录
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetPmExeQryList(GridRequest request)
        {
            //从 BC_CODE 取船机部的部门 ID
            var engineCorpId = (await _dbContext.Query<BC_CODE>(a => a.CODE_TYPE == "engineCorpId")
                .FirstAsync()).CODE_EN;
            //除超管和船机部外，按部门过滤数据
            return await _dbContext.Query<PM_PLAN_EXE>()
                .WhereIf(!_userSession.IsAdmin && _userSession.Corp.CorpID != engineCorpId, a => _userSession.Corp.CorpID == a.DEPT_ID)
                .OrderByDesc(c => c.PLAN_CODE)
                .GetGridData(request);
        }

        #endregion 维保实施查询
    }
}