using Chloe;
using DocumentFormat.OpenXml.Wordprocessing;
using EAM.Device.interfaces;
using Gksyb.Common;
using Gksyb.Core.Application;
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
    public class InventoryTaskService : IInventoryTaskService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxService;
        private readonly UserSession _userSession;
        private readonly ICommonService _iCommonService;
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

        public InventoryTaskService(IDbContext dbContext, IComboxDataService comboxService, UserSession userSession, ICommonService iCommonService)
        {
            _dbContext = dbContext;
            _comboxService = comboxService;
            _userSession = userSession;
            _iCommonService=iCommonService;
        }

        /// <summary>
        /// 下拉
        /// </summary>
        /// <returns></returns>
        public async Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData()
        {
            return await _comboxService.Get(new Dictionary<string, object>(){
                { "ScanStatus",(Expression<Func<BC_CODE, bool>>)(c => "1" == "1")},
                { "DeviceTypeName",(Expression<Func<BASE_DEVICETYPE, bool>>)(c => "1" == "1")},
                { "DeptData",(Expression<Func<CF_DEPT, bool>>)(c => "1" == "1")},
            });
        }

        /// <summary>
        /// 人员下拉
        /// </summary>
        /// <returns></returns>
        public async Task<List<ComboxData>> UserData()
        {
            var corpId = _userSession.Corp.CorpID;
            return await _dbContext.Query<CF_USER>()
               .LeftJoin<CF_USER_PORT>((a, b) => a.LOGINNAME == b.LOGINNAME)
               .LeftJoin<CF_DEPT>((a, b, c) => c.CORPID == b.CORPID)
               .LeftJoin<CF_CORP>((a, b, c, d) => c.CORPID == d.CORPID)
               .Select((a, b, c, d) => new
               ComboxData
               { ID = a.USERID, TEXT = a.REALNAME, VALUE =a.LOGINNAME, EXTEND = d.CORPID })
               .Where(c => c.EXTEND.ToString() == corpId).ToListAsync();
        }

        #region 盘点任务

        /// <summary>
        /// 获取设备盘点任务列表
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetDeviceScanList(GridRequest request)
        {
            return await _dbContext.Query<DEVICE_SCAN>()
                .Where(c => _userSession.Corp.CorpID == c.SEC_DEPTID)
                .OrderBy(c => c.AUDITING)
                .ThenByDesc(c => c.SCAN_CODE)
                .GetGridData(request);
        }

        /// <summary>
        ///  查看设备盘点明细
        /// </summary>
        /// <returns></returns>

        public async Task<AjaxResult> GetDeviceScanDetail(long? ID)
        {
            var qry = await _dbContext.QueryByKeyAsync<DEVICE_SCAN_DET>(ID);
            return AjaxResult.Success(qry);
        }

        /// <summary>
        /// 管理设备盘点任务列表
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManageDeviceScan(SaveRequest<DEVICE_SCAN> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING,
                    c.SCAN_CODE,
                    c.STATUS,
                    c.DEPT_NAME,
                    c.TYPE_NAME,
                    c.USER_NAME,
                    c.SCAN_DATE,
                    c.SEC_DEPT,
                    c.MEMO,
                    c.USER_ID,
                    c.DEPT_ID,
                    c.DEPT_CODE,
                    c.SEC_DEPTID,
                    c.TYPE_ID,
                    c.IDENT,
                    c.SCAN_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                },
                c => a => a.SCAN_ID == c.SCAN_ID, BeforeAdd);
        }

        public async Task BeforeAdd(DEVICE_SCAN entity)
        {
            Random random = new Random();
            int randomNumber = random.Next(1000, 10000);
            entity.SCAN_CODE = "PD"+DateTime.Now.Year+DateTime.Now.ToString("MM")+randomNumber;
            entity.AUDITING = "0";
            entity.STATUS = "1";
            var type_query = await _dbContext.Query<BASE_DEVICETYPE>().Where(c => c.TYPE_ID==entity.TYPE_ID).FirstAsync();
            entity.TYPE_NAME = type_query == null ? "" : type_query.TYPE_NAME;
            var user_query = await _dbContext.Query<CF_USER>().Where(c => c.USERID.ToString()==entity.USER_ID).FirstAsync();
            entity.USER_NAME = user_query == null ? "" : user_query.REALNAME;
            var dept_query = await _dbContext.Query<CF_DEPT>().Where(c => c.DEPT_ID==entity.DEPT_ID).FirstAsync();
            entity.DEPT_NAME = dept_query == null ? "" : dept_query.DEPT_NAME;
            entity.DEPT_CODE = dept_query == null ? "" : dept_query.DEPT_CODE;
            var a_query = await _dbContext.Query<CF_CORP>()
                .LeftJoin<CF_DEPT>((a, b) => a.CORPID==b.CORPID)
                .Select((a, b) => new
                {
                    a.CORPID,
                    a.CORP_SNAME,
                    b.DEPT_ID
                })
                .Where(c => c.DEPT_ID == entity.DEPT_ID).FirstAsync();
            entity.SEC_DEPTID = a_query == null ? "" : a_query.CORPID;
            entity.SEC_DEPT = a_query == null ? "" : a_query.CORP_SNAME;
            entity.SCAN_ID = GuidHelper.NewSnowflakeId().ToString();
        }

        /// <summary>
        /// 生成盘点清单
        /// </summary>
        /// <param name="sid">盘点ID</param>
        /// <param name="deptid">部门ID</param>
        /// <param name="typeid">类型ID</param>
        /// <returns></returns>
        public async Task<AjaxResult> MakeScanList(string sid, string deptid, string typeid)
        {
            //查询盘点任务明细是否有数据
            var qrydet = _dbContext.Query<DEVICE_SCAN_DET>()
                .Select(c => c.SCAN_ID)
                .ToList();
            var qrydetsid = qrydet.Contains(sid);
            if (qrydetsid)
            {
                var deletescans = _dbContext.Query<DEVICE_SCAN_DET>()
                .Select(a => new
                {
                    a.DEVICE_ID,
                    a.DEVICE_NO,
                    a.DEVICE_NAME,
                    a.SEC_DEPT,
                    a.SEC_DEPTID,
                    a.DEPT_ID,
                    a.DEPT_NAME,
                    a.STATUS,
                    a.SCAN_DET_ID,
                    a.SCAN_ID
                })
                .Where(c => qrydet.Contains(sid)).ToList();
                foreach (var deletescan in deletescans)
                {
                    var scandet = new DEVICE_SCAN_DET()
                    {
                        SCAN_DET_ID = deletescan.SCAN_DET_ID,
                        DEVICE_ID = deletescan.DEVICE_ID,
                        SCAN_ID = deletescan.SCAN_ID,
                        DEVICE_NO = deletescan.DEVICE_NO,
                        DEVICE_NAME = deletescan.DEVICE_NAME,
                        DEPT_NAME = deletescan.DEPT_NAME,
                        DEPT_ID = deletescan.DEPT_ID,
                        STATUS = deletescan.STATUS,
                        SEC_DEPTID = deletescan.SEC_DEPTID,
                        SEC_DEPT = deletescan.SEC_DEPT,
                    };
                    var deleteRows = _dbContext.DeleteAsync(scandet);
                }
            }
            var qry = _dbContext.Query<DEVICE_CARD>()
                 .Where(c => c.STATUS == "在用");
            //获取当前部门，子部门的设备卡片

            List<string> corpList = await _iCommonService.GetDeptList(deptid);
            qry = qry.Where(c => corpList.Contains("," + c.DEPT_ID + ","))
                .WhereIf(!_userSession.IsAdmin, a => _userSession.Corp.CorpID == a.SEC_DEPTID)
                .WhereIf(!string.IsNullOrWhiteSpace(typeid), c => c.TYPE_ID == typeid);
            var qrylists = qry
                .Select(a => new
                {
                    a.DEVICE_ID,
                    a.DEVICE_NO,
                    a.DEVICE_NAME,
                    a.SEC_DEPT,
                    a.SEC_DEPTID,
                    a.DEPT_ID,
                    a.DEPT_NAME,
                    a.STATUS,
                })
                .ToList();

            foreach (var qrylist in qrylists)
            {
                var scandet = new DEVICE_SCAN_DET()
                {
                    SCAN_DET_ID = GuidHelper.NewSnowflakeId().ToString(),
                    DEVICE_ID = qrylist.DEVICE_ID,
                    SCAN_ID = sid,
                    DEVICE_NO = qrylist.DEVICE_NO,
                    DEVICE_NAME = qrylist.DEVICE_NAME,
                    DEPT_NAME = qrylist.DEPT_NAME,
                    DEPT_ID = qrylist.DEPT_ID,
                    STATUS = qrylist.STATUS,
                    SEC_DEPTID = qrylist.SEC_DEPTID,
                    SEC_DEPT = qrylist.SEC_DEPT,
                };
                var insertScanId = await _dbContext.InsertAsync(scandet);
            }
            return AjaxResult.Success("成功");
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> Submit(List<string> sids)
        {
            var query = _dbContext.Query<DEVICE_SCAN_DET>()
                 .Where(c => sids.Contains(c.SCAN_ID));
            if (query != null)
            {
                throw new MessageException("盘点任务明细没有数据，必须填写！");
            }
            else
            {
                await _dbContext.UpdateAsync<DEVICE_SCAN>(x => sids.Contains(x.SCAN_ID),
                    x => new DEVICE_SCAN
                    {
                        AUDITING = "1",
                        STATUS = "2",
                    });
                return AjaxResult.Success("成功");
            }
        }



        #endregion 盘点任务

        #region 设备盘点结果
        /// <summary>
        /// 获取设备盘点结果
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetDeviceScanResult(GridRequest request)
        {
            var qry = _dbContext.Query<DEVICE_SCAN>()
                .Where(c => c.STATUS == "1");
            if (!_userSession.IsAdmin)
            {
                qry = qry.Where(c => _userSession.Corp.CorpID == c.SEC_DEPTID);
            }
            return await qry.GetGridData(request);
        }
        #endregion
    }
}