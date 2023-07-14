using Chloe;
using DocumentFormat.OpenXml.Wordprocessing;
using EAM.Device.interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System;
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
                { "DeptData",(Expression<Func<CF_CORP, bool>>)(c => "1" == "1")},
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
                .WhereIf(!_userSession.IsAdmin, a => _userSession.Corp.CorpID == a.SEC_DEPTID)
                .OrderBy(c => c.STATUS)
                .ThenByDesc(c => c.SCAN_CODE)
                .GetGridData(request);
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
            var random = new Random();
            var randomNumber = random.Next(1000, 10000);
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
                var deleteRows = await _dbContext.DeleteAsync<DEVICE_SCAN_DET>(a => a.SCAN_ID==sid);
            }
            //根据部门,类型 获取设备卡片
            var corpPath = _dbContext.Query<CF_CORP>().Where(a => a.CORPID==deptid)
                .Select(c => c.CORP_PATH).ToList().Join();
            var qry = _dbContext.Query<DEVICE_CARD>()
                .WhereIf(!_userSession.IsAdmin, a => _userSession.Corp.CorpID == a.SEC_DEPTID)
                .WhereIf(!string.IsNullOrWhiteSpace(typeid), c => c.TYPE_ID == typeid)
                .LeftJoin<CF_CORP>((a, b) => a.SEC_DEPTID==b.CORPID)
                .Where((a, b) => (","+b.CORP_PATH).Contains(","+corpPath));
            if (qry != null)
            {
                var qrylists = qry
                .Select((a, b) => new
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
                        CREATE_USERID = _userSession.UserID.ToString(),
                        CREATEDATE = Sysdate,
                    };
                    var insertScanId = await _dbContext.InsertAsync(scandet);
                }
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
            if (query == null)
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

        #region 设备盘点任务明细

        /// <summary>
        ///  查看设备盘点明细
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetDeviceScanDetails(GridRequest request)
        {
            var info = await _dbContext.Query<DEVICE_SCAN_DET>()
                .LeftJoin<DEVICE_CARD>((a, b) => a.DEVICE_ID==b.DEVICE_ID)
                .Select((a, b) => new
                {
                    a.SCAN_ID,
                    a.HANDLE,
                    a.SCAN_RESULT,
                    a.DEVICE_NO,
                    a.DEVICE_NAME,
                    a.DEPT_NAME,
                    a.STATUS,
                    a.SEC_DEPT,
                    a.SCAN_DET_ID,
                    a.MEMO,
                    b.TYPE_NAME,
                    b.DEVICE_TYPE,
                    b.ASSET_CODE,
                })
                .GetGridData(request);
            return info;
        }

        #endregion 设备盘点任务明细

        #region 设备盘点结果

        /// <summary>
        /// 获取设备盘点结果
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetDeviceScanResult(GridRequest request)
        {
            return await _dbContext.Query<DEVICE_SCAN>()
                .WhereIf(!_userSession.IsAdmin, a => _userSession.Corp.CorpID == a.SEC_DEPTID)
                .Where(c => c.AUDITING == "1")
                .OrderBy(c => c.STATUS)
                .ThenByDesc(c => c.SCAN_CODE)
                .GetGridData(request);
        }

        /// <summary>
        /// 管理设备盘点任务明细列表
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManageScanDetail(SaveRequest<DEVICE_SCAN_DET> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.HANDLE,
                    c.SCAN_RESULT,
                    c.MEMO,
                    c.SCAN_DET_ID
                },
                c => a => a.SCAN_DET_ID == c.SCAN_DET_ID, null, BeforeUpdate);
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(DEVICE_SCAN_DET entity)
        {
            if (entity.HANDLE=="0")
            {
                entity.HANDLE = "1";
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> SubmitScanDet(string sid)
        {
            var queryScan = _dbContext.Query<DEVICE_SCAN>()
                 .Where(c => c.SCAN_ID==sid).Select(c => c.STATUS).First();
            if (queryScan=="3")
            {
                throw new MessageException("已经盘点完成，无法再次提交！");
            }
            var query = _dbContext.Query<DEVICE_SCAN_DET>()
                 .Where(c => c.SCAN_ID==sid).Select(c =>
                     c.HANDLE
                 ).ToList();
            if (query.Contains("0"))
            {
                throw new MessageException("必须处理所有盘点结果！");
            }
            else
            {
                await _dbContext.UpdateAsync<DEVICE_SCAN>(x => x.SCAN_ID==sid,
                    x => new DEVICE_SCAN
                    {
                        STATUS = "3",
                    });
                var random = new Random();
                var randomNumber = random.Next(1000, 10000);
                var queryups = _dbContext.Query<DEVICE_SCAN_DET>()
                 .Where(c => c.SCAN_ID==sid&&c.SCAN_RESULT=="盘盈").ToList();
                if (queryups!=null)
                {
                    foreach (var queryup in queryups)
                    {
                        var scandetre = new DEVICE_SCAN_RESULT()
                        {
                            RESULT_ID = GuidHelper.NewSnowflakeId().ToString(),
                            AUDITING = "0",
                            SCAN_ID = sid,
                            SCAN_CODE = "PY"+DateTime.Now.Year+DateTime.Now.ToString("MM")+randomNumber,
                            SCAN_DATE = Sysdate,
                            SCAN_TYPE = "盘盈",
                            DEVICE_NO = queryup.DEVICE_NO,
                            DEVICE_NAME = queryup.DEVICE_NAME,
                            DEPT_NAME = queryup.DEPT_NAME,
                            DEPT_ID = queryup.DEPT_ID,
                            STATUS = queryup.STATUS,
                            SEC_DEPTID = queryup.SEC_DEPTID,
                            SEC_DEPT = queryup.SEC_DEPT,
                            DEVICE_ID = queryup.DEVICE_ID,
                            MEMO = queryup.MEMO,
                            CREATE_USERID = _userSession.UserID.ToString(),
                            CREATEDATE = Sysdate,
                        };
                        var insertScandetre = await _dbContext.InsertAsync(scandetre);
                    }
                }
                var querydowns = _dbContext.Query<DEVICE_SCAN_DET>()
                 .Where(c => c.SCAN_ID==sid&&c.SCAN_RESULT=="盘亏").ToList();
                if (querydowns != null)
                {
                    foreach (var querydown in querydowns)
                    {
                        var scandetre = new DEVICE_SCAN_RESULT()
                        {
                            RESULT_ID = GuidHelper.NewSnowflakeId().ToString(),
                            AUDITING = "0",
                            SCAN_ID = sid,
                            SCAN_CODE = "PK"+DateTime.Now.Year+DateTime.Now.ToString("MM")+randomNumber,
                            SCAN_DATE = Sysdate,
                            SCAN_TYPE = "盘亏",
                            DEVICE_NO = querydown.DEVICE_NO,
                            DEVICE_NAME = querydown.DEVICE_NAME,
                            DEPT_NAME = querydown.DEPT_NAME,
                            DEPT_ID = querydown.DEPT_ID,
                            STATUS = querydown.STATUS,
                            SEC_DEPTID = querydown.SEC_DEPTID,
                            SEC_DEPT = querydown.SEC_DEPT,
                            DEVICE_ID = querydown.DEVICE_ID,
                            MEMO = querydown.MEMO,
                            CREATE_USERID = _userSession.UserID.ToString(),
                            CREATEDATE = Sysdate,
                        };
                        var insertScandetre = await _dbContext.InsertAsync(scandetre);
                    }
                }
                return AjaxResult.Success("成功");
            }
        }

        #endregion 设备盘点结果

        #region 设备盈亏记录
        /// <summary>
        /// 获取盈亏记录列表
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetUpDownList(GridRequest request)
        {
            return await _dbContext.Query<DEVICE_SCAN_RESULT>()
                .WhereIf(!_userSession.IsAdmin, a => _userSession.Corp.CorpID == a.SEC_DEPTID)
                .OrderBy(c => c.STATUS)
                .ThenByDesc(c => c.SCAN_CODE)
                .GetGridData(request);
        }

        /// <summary>
        /// 管理设备盈亏记录
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManageUpDown(SaveRequest<DEVICE_SCAN_RESULT> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.RESULT_ID,
                    c.RESULT_MEMO,
                },
                c => a => a.RESULT_ID == c.RESULT_ID, null, BeforeUpdate);
        }
        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(DEVICE_SCAN_RESULT entity)
        {
            entity.AUDITING = "1";
            await Task.CompletedTask;
        }
        #endregion

    }
}