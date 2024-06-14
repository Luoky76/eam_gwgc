using Chloe;
using EAM.Device.interfaces;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
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
        private readonly ICorpService _corpService;
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

        public InventoryTaskService(IDbContext dbContext, ICorpService corpService, IComboxDataService comboxService, UserSession userSession)
        {
            _dbContext = dbContext;
            _corpService = corpService;
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
                { "BCCode", "deviceType" },
                { "ScanStatus",(Expression<Func<BC_CODE, bool>>)(c => "1" == "1")},
                { "DeviceTypeName",(Expression<Func<BASE_DEVICETYPE, bool>>)(c => "1" == "1")},
                { "AssetStatus",(Expression<Func<BC_CODE, bool>>)(c => "1" == "1")},
                { "DeptData",(Expression<Func<CF_CORP, bool>>)(a => a.CORPID == _userSession.ParentCompany.CorpID)},
            });
        }

        #region 盘点任务

        /// <summary>
        /// 获取设备盘点任务列表
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetDeviceScanList(GridRequest request)
        {
            //从 BC_CODE 取船机部的部门 ID
            var engineCorpId = (await _dbContext.Query<BC_CODE>(a => a.CODE_TYPE == "engineCorpId")
                .FirstAsync()).CODE_EN;
            //除超管和船机部外，按部门过滤数据
            return await _dbContext.Query<DEVICE_SCAN>()
                .WhereIf(!_userSession.IsAdmin && _userSession.Corp.CorpID != engineCorpId, a => _userSession.Corp.CorpID == a.DEPT_ID)
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
            //查询盘点任务明细是否有数据
            if ((request.Updated?.Count ?? 0) > 0 || (request.Deleted?.Count ?? 0) > 0)
            {
                var scanIds = request.Updated?.Select(c => c.SCAN_ID).ToList();

                if (scanIds?.Count > 0)
                {
                    var deleteRows = await _dbContext.DeleteAsync<DEVICE_SCAN_DET>(a => scanIds.Contains(a.SCAN_ID));
                }
            }
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
            string aa = "PD" + DateTime.Now.ToString("yyyyMM");
            string def = aa + "0000";
            var model = await _dbContext.Query<DEVICE_SCAN>(x => x.SCAN_CODE.Contains(aa)).Select(x => Sql.Max(x.SCAN_CODE) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(8, 4).CastTo<int>() + 1;
            entity.SCAN_CODE = aa + index.ToString("D4");
            entity.AUDITING = "0";
            entity.STATUS = "1";
            entity.SEC_DEPTID = _userSession.ParentCompany.CorpID;
            entity.SEC_DEPT = _userSession.ParentCompany.CName;
            entity.DEPT_ID = _userSession.Corp.CorpID;
            entity.DEPT_NAME = _userSession.Corp.CName;
            entity.SCAN_ID = GuidHelper.NewSnowflakeId().ToString();
        }
        // 递归获取分类及其子分类的ID集合
        private List<string> GetChildTypeIds(string pretypeid)
        {
            var childTypeIds = new List<string>
            {
                // 添加当前分类的ID到childTypeIds
                pretypeid
            };
            // 查询所有子分类的ID并将它们添加到 childTypeIds
            void GetChildren(string parentId)
            {
                var children = _dbContext.Query<BASE_DEVICETYPE>()
                    .Where(c => c.PRE_TYPEID == parentId)
                    .Select(c => c.TYPE_ID)
                    .ToList();

                childTypeIds.AddRange(children);

                foreach (var child in children)
                {
                    GetChildren(child);
                }
            }

            GetChildren(pretypeid);

            return childTypeIds;
        }

        /// <summary>
        /// 生成盘点清单
        /// </summary>
        /// <param name="sid">盘点ID</param>
        /// <param name="deptid">部门ID</param>
        /// <param name="typeid">类型ID</param>
        /// <returns></returns>
        public async Task<string> MakeScanList(string sid, string deptid, string typeid)
        {
            //查询盘点任务明细是否有数据
            var qrydet = _dbContext.Query<DEVICE_SCAN_DET>()
                .Select(c => c.SCAN_ID)
                .ToList();
            var qrydetsid = qrydet.Contains(sid);
            if (qrydetsid)
            {
                var deleteRows = await _dbContext.DeleteAsync<DEVICE_SCAN_DET>(a => a.SCAN_ID == sid);
            }
            //根据部门,类型 获取设备卡片
            var corpPath = _dbContext.Query<CF_CORP>().Where(a => a.CORPID == deptid)
                .Select(c => c.CORP_PATH).ToList().Join();
            //从 BC_CODE 取船机部的部门 ID
            var engineCorpId = (await _dbContext.Query<BC_CODE>(a => a.CODE_TYPE == "engineCorpId")
                .FirstAsync()).CODE_EN;
            //除超管和船机部外，按部门过滤数据
            //获取分类及其子分类的ID集合
            var qry = _dbContext.Query<DEVICE_CARD>()
                .WhereIf(!_userSession.IsAdmin && _userSession.Corp.CorpID != engineCorpId, a => _userSession.Corp.CorpID == a.DEPT_ID)
                .WhereIf(!string.IsNullOrWhiteSpace(typeid), c => typeid == c.TYPE_ID)
                .LeftJoin<CF_CORP>((a, b) => a.DEPT_ID == b.CORPID)
                .Where((a, b) => b.CORP_PATH.StartsWith(corpPath));
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
                var dsdList = new List<DEVICE_SCAN_DET>();
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
                        HANDLE = "0",
                    };
                    dsdList.Add(scandet);
                }
                await _dbContext.InsertRangeAsync(dsdList);
            }

            return "";
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        public async Task<int> Submit(List<string> sids)
        {
            var query = _dbContext.Query<DEVICE_SCAN_DET>()
                 .Where(c => sids.Contains(c.SCAN_ID)).First();
            if (query == null)
            {
                throw new MessageException("盘点任务明细没有数据，必须填写！");
            }
            else
            {
                var updatedevice = await _dbContext.UpdateAsync<DEVICE_SCAN>(x => sids.Contains(x.SCAN_ID),
                    x => new DEVICE_SCAN
                    {
                        AUDITING = "1",
                        STATUS = "2",
                    });
                return updatedevice;
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
                .LeftJoin<DEVICE_CARD>((a, b) => a.DEVICE_ID == b.DEVICE_ID)
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
            //从 BC_CODE 取船机部的部门 ID
            var engineCorpId = (await _dbContext.Query<BC_CODE>(a => a.CODE_TYPE == "engineCorpId")
                .FirstAsync()).CODE_EN;
            //除超管和船机部外，按部门过滤数据
            return await _dbContext.Query<DEVICE_SCAN>()
                .WhereIf(!_userSession.IsAdmin && _userSession.Corp.CorpID != engineCorpId, a => _userSession.Corp.CorpID == a.DEPT_ID)
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
            var queryScan = _dbContext.Query<DEVICE_SCAN>()
                     .Where(c => c.SCAN_ID == entity.SCAN_ID).Select(c => c.STATUS).First();
            if (queryScan == "3")
            {
                throw new MessageException("已经盘点完成，无法保存！");
            }
            if (entity.SCAN_RESULT != null)
            {
                entity.HANDLE = "1";
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 提交盘点任务结果
        /// </summary>
        /// <returns></returns>
        public async Task<string> SubmitScanDet(string sid)
        {
            try
            {
                var queryScan = _dbContext.Query<DEVICE_SCAN>()
                     .Where(c => c.SCAN_ID == sid).Select(c => c.STATUS).First();
                if (queryScan == "3")
                {
                    throw new MessageException("已经盘点完成，无法再次提交！");
                }
                var query = _dbContext.Query<DEVICE_SCAN_DET>()
                     .Where(c => c.SCAN_ID == sid).Select(c =>
                         c.HANDLE
                     ).ToList();
                if (query.Contains("0"))
                {
                    throw new MessageException("必须处理所有盘点结果！");
                }
                else
                {
                    await _dbContext.UpdateAsync<DEVICE_SCAN>(x => x.SCAN_ID == sid,
                       x => new DEVICE_SCAN
                       {
                           STATUS = "3",
                       });

                    string aa = DateTime.Now.ToString("yyyyMM");
                    string def = aa + "0000";
                    var queryups = _dbContext.Query<DEVICE_SCAN_DET>()
                     .Where(c => c.SCAN_ID == sid && c.SCAN_RESULT != "正常").ToList();
                    if (queryups != null)
                    {
                        var scandetreList = new List<DEVICE_SCAN_RESULT>();
                        var scan_code = 0;
                        var scan_type = "";
                        var pyk = "";
                        foreach (var queryup in queryups)
                        {
                            scan_type = queryup.SCAN_RESULT == "盘盈" ? "盘盈" : "盘亏";
                            pyk = queryup.SCAN_RESULT == "盘盈" ? "PY" : "PK";
                            var scanTypeCount = scandetreList.Count(item => item.SCAN_TYPE == scan_type);
                            var scanQuery = _dbContext.Query<DEVICE_SCAN_RESULT>(x => x.SCAN_CODE.Contains(aa) && x.SCAN_TYPE == scan_type);
                            var maxScanCode = await scanQuery.Select(x => Sql.Max(x.SCAN_CODE) ?? def).FirstOrDefaultAsync();
                            scan_code = maxScanCode.SubStr(8, 4).CastTo<int>() + (scanTypeCount > 0 ? scanTypeCount + 1 : 1);
                            var scandetre = new DEVICE_SCAN_RESULT()
                            {
                                RESULT_ID = GuidHelper.NewSnowflakeId().ToString(),
                                AUDITING = "0",
                                SCAN_ID = sid,
                                SCAN_CODE = pyk + aa + scan_code.ToString("D4"),
                                SCAN_DATE = Sysdate,
                                SCAN_TYPE = scan_type,
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
                            scandetreList.Add(scandetre);
                        }
                        await _dbContext.InsertRangeAsync(scandetreList);
                    }
                    return "";
                }
            }
            catch (Exception e)
            {
                throw new Exception("原因：" + e.Message);
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
            //从 BC_CODE 取船机部的部门 ID
            var engineCorpId = (await _dbContext.Query<BC_CODE>(a => a.CODE_TYPE == "engineCorpId")
                .FirstAsync()).CODE_EN;
            //除超管和船机部外，按部门过滤数据
            return await _dbContext.Query<DEVICE_SCAN_RESULT>()
                .WhereIf(!_userSession.IsAdmin && _userSession.Corp.CorpID != engineCorpId, a => _userSession.Corp.CorpID == a.DEPT_ID)
                .OrderBy(c => c.AUDITING)
                .ThenBy(c => c.STATUS)
                .ThenByDesc(c => c.SCAN_CODE)
                .GetGridData(request);
        }

        /// <summary>
        /// 提交设备盈亏记录
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> SubmitUpDown(string sid)
        {
            await _dbContext.UpdateAsync<DEVICE_SCAN_RESULT>(x => x.RESULT_ID == sid,
                x => new DEVICE_SCAN_RESULT
                {
                    AUDITING = "1",
                });
            var scanid = await _dbContext.Query<DEVICE_SCAN_RESULT>(x => x.RESULT_ID == sid)
                .Select(c => c.SCAN_ID).FirstOrDefaultAsync();
            var qry = await _dbContext.Query<DEVICE_SCAN>()
                .LeftJoin<DEVICE_SCAN_RESULT>((a, b) => a.SCAN_ID == b.SCAN_ID)
                .Where((a, b) => a.SCAN_ID == scanid).Select((a, b) => b.AUDITING).ToListAsync();
            if (!qry.Contains("0"))
            {
                await _dbContext.UpdateAsync<DEVICE_SCAN>(x => x.SCAN_ID == scanid,
                x => new DEVICE_SCAN
                {
                    STATUS = "4",
                });
            }
            return AjaxResult.Success();
        }

        /// <summary>
        /// 反提交设备盈亏记录
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> UnSubmitUpDown(string sid)
        {
            await _dbContext.UpdateAsync<DEVICE_SCAN_RESULT>(x => x.RESULT_ID == sid,
                x => new DEVICE_SCAN_RESULT
                {
                    AUDITING = "0",
                });
            var scanid = await _dbContext.Query<DEVICE_SCAN_RESULT>(x => x.RESULT_ID == sid)
                .Select(c => c.SCAN_ID).FirstOrDefaultAsync();
            var qry = await _dbContext.Query<DEVICE_SCAN>()
                .LeftJoin<DEVICE_SCAN_RESULT>((a, b) => a.SCAN_ID == b.SCAN_ID)
                .Where((a, b) => a.SCAN_ID == scanid).Select((a, b) => b.AUDITING).ToListAsync();
            if (qry.Contains("0"))
            {
                await _dbContext.UpdateAsync<DEVICE_SCAN>(x => x.SCAN_ID == scanid,
                x => new DEVICE_SCAN
                {
                    STATUS = "3",
                });
            }
            return AjaxResult.Success();
        }

        /// <summary>
        /// 保存设备盈亏记录
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
                c => a => a.RESULT_ID == c.RESULT_ID);
        }

        #endregion 设备盈亏记录
    }
}