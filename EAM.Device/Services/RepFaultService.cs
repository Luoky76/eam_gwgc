using Chloe;
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
    public class RepFaultService : IBaseService
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
        public RepFaultService(IDbContext dbContext, IComboxDataService comboxService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxService = comboxService;
            _userSession = userSession;
        }

        /// <summary>
        /// 下拉
        /// </summary>
        /// <returns></returns>
        public async Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxDataAsync()
        {
            return await _comboxService.Get(new Dictionary<string, object>(){
                { "DisposeType",null},
                { "DeviceStatus",null},
                { "RepType",null},
                { "FaultSrc",null},
                { "FaultStatus",null},
                { "MaintDept",null},
                { "DeviceInfo",(Expression<Func<DEVICE_CARD, bool>>)(c => c.TYPE_ID == "2")},
                { "ShipInfo",null},
            });
        }

        #region 故障处理

        /// <summary>
        /// 导入功能
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> ImportList(GridRequest request)
        {
            return await _dbContext.Query<SP_STORE>()
                .Where(c => c.NUM > 0)
                .GetGridData(request);
        }

        /// <summary>
        /// 获取故障处理记录
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetFaultExeList(GridRequest request)
        {
            //从 BC_CODE 取船机部的部门 ID
            var engineCorpId = (await _dbContext.Query<BC_CODE>(a => a.CODE_TYPE == "engineCorpId")
                .FirstAsync()).CODE_EN;
            //除超管和船机部外，按部门过滤数据
            return await _dbContext.Query<REP_FAULT>()
                .WhereIf(!_userSession.IsAdmin && _userSession.Corp.CorpID != engineCorpId, a => _userSession.Corp.CorpID == a.DEPT_ID)
                .OrderBy(c => c.AUDITING_B)
                .ThenByDesc(c => c.FAULT_CODE)
                .GetGridData(request);
        }
        //图片SRC
        public class REP_FAULT_IMG : REP_FAULT
        {
            public List<string> ImageSrcList { get; set; }

        }
        /// <summary>
        /// 获取单条故障处理记录
        /// </summary>
        /// <returns></returns>

        public async Task<REP_FAULT_IMG> GetFaultExeListDetail(string ID)
        {
            var qry = await _dbContext.QueryByKeyAsync<REP_FAULT_IMG>(ID);
            qry.ImageSrcList = await _dbContext.Query<SYS_ATTACH>()
                      .Where(img => img.data_id == ID && img.attach_type == ".jpg" || img.attach_type == ".jpeg")
                      .Select(img => img.attach_path)
                      .ToListAsync();

            return qry;
        }

        /// <summary>
        /// 管理故障处理记录
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManageFaultExe(SaveRequest<REP_FAULT> request)
        {
            await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING_B,
                    c.FAULT_CODE,
                    c.FAULT_STATUS,
                    c.SHIP_ID,
                    c.SHIP_CODE,
                    c.SHIP_NAME,
                    c.FAULT_DATE,
                    c.DEVICE_STATUS,
                    c.DEVICE_NAME,
                    c.DEVICE_CODE,
                    c.DEVICE_TYPE,
                    c.TYPE_NAME,
                    c.FAULT_SRC,
                    c.SRC_CODE,
                    c.DEPT_NAME,
                    c.DEPT_ID,
                    c.WDEPT_NAME,
                    c.WDEPT_ID,
                    c.EDIT_USER,
                    c.EDIT_USERID,
                    c.FAULT_DESCRIBE,
                    c.FAULT_DISPOSE,
                    c.ORDER_USER,
                    c.ORDER_USERID,
                    c.NOTICE_TIME,
                    c.DISPOSE_TYPE,
                    c.REP_TYPE_NAME,
                    c.REP_TYPE_ID,
                    c.FRDB_CODE,
                    c.ORDER_DATE,
                    c.DEVICE_ID,
                    c.COMPLETE_DATE,
                    c.REPAIR_HOURS,
                    c.FAULT_REASON,
                    c.MEASURES,
                    c.FAULT_MEMO,
                    c.FAULT_ID,
                    c.REPAIR_USERID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                },
                c => a => a.FAULT_ID == c.FAULT_ID, BeforeAdd);
            var ID = "";
            if (request.Added.Count > 0)
            {
                ID = request.Added[0].FAULT_ID;
            }
            else
            {
                ID = request.Updated[0].FAULT_ID;
            }
            return AjaxResult.Success(ID);
        }

        /// <summary>
        /// 新增前处理
        /// </summary>
        public async Task BeforeAdd(REP_FAULT entity)
        {
            entity.SEC_DEPTID = _userSession.ParentCompany.CorpID;
            entity.SEC_DEPT = _userSession.ParentCompany.CName;
            entity.DEPT_ID = _userSession.Corp.CorpID;
            entity.DEPT_NAME = _userSession.Corp.CName;
            //维修人当前登陆人
            entity.REPAIR_USER = _userSession.UserName;
            entity.REPAIR_USERID = _userSession.UserID.ToString();
            entity.EDIT_USER = _userSession.UserName;
            entity.EDIT_USERID = _userSession.UserID.ToString();
            entity.FAULT_SRC = "10";
            entity.FAULT_STATUS = "30";
            string aa = "GZ" + DateTime.Now.ToString("yyyyMM");
            string def = aa + "0000";
            var model = await _dbContext.Query<REP_FAULT>(x => x.FAULT_CODE.Contains(aa)).Select(x => Sql.Max(x.FAULT_CODE) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(8, 4).CastTo<int>() + 1;
            entity.FAULT_CODE = aa + index.ToString("D4");
            entity.AUDITING_B = "0";
            entity.FAULT_ID = GuidHelper.NewSnowflakeId().ToString();
        }

        /// <summary>
        /// 提交故障处理
        /// </summary>
        /// <returns></returns>
        public async Task<int> SubmitFaultExe(List<string> sids)
        {
            return await _dbContext.UpdateAsync<REP_FAULT>(x => sids.Contains(x.FAULT_ID),
                x => new REP_FAULT
                {
                    AUDITING_B = "1",
                    FAULT_STATUS = "40",
                });
        }

        /// <summary>
        /// 反提交故障处理
        /// </summary>
        /// <returns></returns>
        public async Task<int> UnSubmitFaultExe(string sid)
        {
            var qryfaultitem = await _dbContext.Query<REP_FAULT>(x => sid == x.FAULT_ID)
                .Select(c => c.AUDITING_D)
                .FirstOrDefaultAsync();
            if (qryfaultitem == "1")
            {
                throw new MessageException("故障已验收，不可撤销提交！");
            }
            else
            {
                return await _dbContext.UpdateAsync<REP_FAULT>(x => sid == x.FAULT_ID,
                x => new REP_FAULT
                {
                    AUDITING_B = "0",
                    FAULT_STATUS = "30",
                });
            }
        }

        /// <summary>
        /// 获取人员明细
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetFaultPepList(GridRequest request)
        {
            return await _dbContext.Query<REP_FAULT_LABOR>()
                .GetGridData(request);
        }

        /// <summary>
        /// 获取物资明细
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetFaultSpList(GridRequest request)
        {
            return await _dbContext.Query<REP_FAULT_SP>()
                .GetGridData(request);
        }

        /// <summary>
        /// 管理人员明细
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManageFaultPep(SaveRequest<REP_FAULT_LABOR> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.MEMO,
                    c.LABOR_NAME,
                    c.WORK_HOURS,
                    c.FAULT_LABOR_ID,
                },
                c => a => a.FAULT_LABOR_ID == c.FAULT_LABOR_ID, BeforeAdd);
        }

        /// <summary>
        /// 新增前处理
        /// </summary>
        public async Task BeforeAdd(REP_FAULT_LABOR entity)
        {
            entity.FAULT_LABOR_ID = GuidHelper.NewSnowflakeId().ToString();
            await Task.CompletedTask;
        }

        /// <summary>
        /// 管理物资明细
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManageFaultSp(SaveRequest<REP_FAULT_SP> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.REAL_OUT_NUM,
                    c.FAULT_SP_ID,
                },
                c => a => a.FAULT_SP_ID == c.FAULT_SP_ID, BeforeAdd);
        }
        /// <summary>
        /// 新增前处理
        /// </summary>
        public async Task BeforeAdd(REP_FAULT_SP entity)
        {
            entity.FAULT_SP_ID = GuidHelper.NewSnowflakeId().ToString();
            await Task.CompletedTask;
        }
        #endregion
        #region 故障验收
        /// <summary>
        /// 提交验收
        /// </summary>
        /// <returns></returns>
        public async Task<int> SubmitFaultCheck(List<string> sids)
        {
            var querys = _dbContext.Query<REP_FAULT>()
                 .Where(c => sids.Contains(c.FAULT_ID)).Select(c =>
                     c.CHECK_RESULT
                 ).ToList();
            if (querys.Contains(null))
            {
                throw new MessageException("必须处理所有验收结果！");
            }
            else
            {
                var qrylists = _dbContext.Query<REP_FAULT>()
                 .Where(c => sids.Contains(c.FAULT_ID)).ToList();
                foreach (var qrylist in qrylists)
                {
                    string aa = "GZK" + DateTime.Now.ToString("yyyyMM");
                    string def = aa + "0000";
                    var model = await _dbContext.Query<REP_FRDB>(x => x.FRDB_CODE.Contains(aa)).Select(x => Sql.Max(x.FRDB_CODE) ?? def).FirstOrDefaultAsync();
                    var index = model.SubStr(9, 4).CastTo<int>() + 1;
                    var scandet = new REP_FRDB()
                    {
                        FRDB_ID = GuidHelper.NewSnowflakeId().ToString(),
                        FRDB_CODE = aa + index.ToString("D4"),
                        DEVICE_CODE = qrylist.SHIP_CODE,
                        DEVICE_ID = qrylist.SHIP_ID,
                        FAULT_DESCRIBE = qrylist.FAULT_DESCRIBE,
                        REP_DEVICE = qrylist.DEVICE_NAME,
                        SRC_FUNCTION = qrylist.FAULT_SRC,
                        SRC_CODE = qrylist.FAULT_CODE,
                        FRDB_LEVEL = "10",
                        FAULT_REASON = qrylist.FAULT_REASON,
                        MEASURES = qrylist.MEASURES,
                        AUDITING = "0",
                        CREATE_USERID = _userSession.UserID.ToString(),
                        EDIT_USERID = _userSession.UserID.ToString(),
                        CREATEDATE = Sysdate,
                        EDIT_DATE = Sysdate,
                    };
                    var insertScanId = await _dbContext.InsertAsync(scandet);
                }
                return await _dbContext.UpdateAsync<REP_FAULT>(x => sids.Contains(x.FAULT_ID),
                    x => new REP_FAULT
                    {
                        AUDITING_D = "1",
                        FAULT_STATUS = "50",
                    });
            }
        }
        /// <summary>
        /// 反提交验收
        /// </summary>
        /// <returns></returns>
        public async Task<int> UnSubmitFaultCheck(string sid)
        {
            var qryCode = _dbContext.Query<REP_FAULT>()
             .Where(c => sid == c.FAULT_ID).Select(c => c.FAULT_CODE).FirstOrDefault();
            await _dbContext.DeleteAsync<REP_FRDB>(c => c.SRC_CODE == qryCode);
            return await _dbContext.UpdateAsync<REP_FAULT>(x => sid == x.FAULT_ID,
                x => new REP_FAULT
                {
                    AUDITING_D = "0",
                    FAULT_STATUS = "40",
                });
        }

        /// <summary>
        /// 驳回验收
        /// </summary>
        /// <returns></returns>
        public async Task<int> ReturnedFaultUnCheck(List<string> sids)
        {
            return await _dbContext.UpdateAsync<REP_FAULT>(x => sids.Contains(x.FAULT_ID),
                x => new REP_FAULT
                {
                    AUDITING_B = "0",
                    FAULT_STATUS = "30",
                });
        }
        /// <summary>
        /// 管理验收结果
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManageFaultCheck(SaveRequest<REP_FAULT> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.CHECK_RESULT,
                    c.CHECK_MEMO,
                },
                c => a => a.FAULT_ID == c.FAULT_ID, BeforeAddCheck);
        }

        /// <summary>
        /// 新增前处理
        /// </summary>
        public async Task BeforeAddCheck(REP_FAULT entity)
        {
            entity.CHECK_USER = _userSession.UserName;
            entity.CHECK_USERID = _userSession.UserID.ToString();
            entity.CHECK_DATE = Sysdate;
            await Task.CompletedTask;
        }
        /// <summary>
        /// 获取验收记录
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetFaultCheckList(GridRequest request)
        {
            //从 BC_CODE 取船机部的部门 ID
            var engineCorpId = (await _dbContext.Query<BC_CODE>(a => a.CODE_TYPE == "engineCorpId")
                .FirstAsync()).CODE_EN;
            //除超管和船机部外，按部门过滤数据
            return await _dbContext.Query<REP_FAULT>()
                .WhereIf(!_userSession.IsAdmin && _userSession.Corp.CorpID != engineCorpId, a => _userSession.Corp.CorpID == a.DEPT_ID)
                .Where(c => c.DISPOSE_TYPE == "2" && (c.AUDITING_B == "1" || c.AUDITING_C == "1"))
                .OrderBy(c => c.AUDITING_D)
                .ThenByDesc(c => c.FAULT_CODE)
                .GetGridData(request);
        }
        #endregion
        #region 验收查询
        /// <summary>
        /// 获取验收查询记录
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetFaultCheckQryList(GridRequest request)
        {
            //从 BC_CODE 取船机部的部门 ID
            var engineCorpId = (await _dbContext.Query<BC_CODE>(a => a.CODE_TYPE == "engineCorpId")
                .FirstAsync()).CODE_EN;
            //除超管和船机部外，按部门过滤数据
            return await _dbContext.Query<REP_FAULT>()
                .WhereIf(!_userSession.IsAdmin && _userSession.Corp.CorpID != engineCorpId, a => _userSession.Corp.CorpID == a.DEPT_ID)
                .OrderBy(c => c.FAULT_STATUS)
                .ThenByDesc(c => c.FAULT_CODE)
                .GetGridData(request);
        }
        #endregion
    }
}