using Chloe;
using DocumentFormat.OpenXml.InkML;
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

namespace EAM.Device.services
{
    public class DeviceRunService : IDeviceRunService
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
        public DeviceRunService(IDbContext dbContext, IComboxDataService comboxService, UserSession userSession)
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
                { "RunStatus",null},
            });
        }

        /// <summary>
        /// 获取设备卡片基础信息
        /// </summary>
        /// <returns></returns>
        public async Task<List<ComboxData>> DeviceData()
        {
            //取设备运行状态分组第一条
            var detail = _dbContext.Query<RUN_TRANS>(a => a.AUDITING=="1").Select(x => new
            {
                x.DEVICE_ID,
                x.SUBMITDATE,
            }).GroupBy(x => new
            {
                x.DEVICE_ID,
            }).Select(x => new
            {
                x.DEVICE_ID,
                SUBMITDATE = Sql.Max(x.SUBMITDATE),
            });
            //从 BC_CODE 取船机部的部门 ID
            var engineCorpId = (await _dbContext.Query<BC_CODE>(a => a.CODE_TYPE == "engineCorpId")
                .FirstAsync()).CODE_EN;
            //除超管和船机部外，按部门过滤数据
            var qry = _dbContext.Query<DEVICE_CARD>()
                .WhereIf(!_userSession.IsAdmin && _userSession.Corp.CorpID != engineCorpId, a => _userSession.Corp.CorpID == a.DEPT_ID)
                .LeftJoin(detail, (a, b) => a.DEVICE_ID == b.DEVICE_ID)
                .LeftJoin<RUN_TRANS>((a, b, c) => b.DEVICE_ID == c.DEVICE_ID && b.SUBMITDATE == c.SUBMITDATE && c.AUDITING=="1")
                .Where((a, b, c) => a.AUDITING=="1"&&a.STATUS=="1"&&a.TYPE_ID=="2");
            return await qry
                .Select((a, b, c) => new ComboxData()
                {
                    ID = a.DEVICE_ID,
                    TEXT = a.DEVICE_NAME,
                    VALUE = a.DEVICE_NO,
                    EXTEND =c.NEW_RUN_STATUS ?? "正常",
                    EXTEND1 =a.DEVICE_TYPE,
                    EXTEND2 =a.TYPE_NAME,
                })
               .ToListAsync();
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetRun(GridRequest request)
        {
            //从 BC_CODE 取船机部的部门 ID
            var engineCorpId = (await _dbContext.Query<BC_CODE>(a => a.CODE_TYPE == "engineCorpId")
                .FirstAsync()).CODE_EN;
            //除超管和船机部外，按部门过滤数据
            var qry = _dbContext.Query<RUN_TRANS>()
                .WhereIf(!_userSession.IsAdmin && _userSession.Corp.CorpID != engineCorpId, a => _userSession.Corp.CorpID == a.DEPT_ID)
                .OrderBy(c => c.AUDITING)
                .ThenByDesc(c => c.TRANS_DATE);
            return await qry.GetGridData(request);
        }

        /// <summary>
        /// 获取单条记录
        /// </summary>
        /// <returns></returns>

        public async Task<RUN_TRANS> GetRunDetail(string ID)
        {
            var qry = await _dbContext.QueryByKeyAsync<RUN_TRANS>(ID);
            return qry;
        }

        /// <summary>
        /// 增删改
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> Manage(SaveRequest<RUN_TRANS> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING,
                    c.TRANS_ID,
                    c.TRANS_DATE,
                    c.DEVICE_NO,
                    c.DEVICE_NAME,
                    c.RUN_STATUS,
                    c.NEW_RUN_STATUS,
                    c.TRANS_MEMO,
                    c.DEPT_NAME,
                    c.SEC_DEPT,
                },
                c => a => a.TRANS_ID == c.TRANS_ID, BeforeAdd);
        }

        private async Task BeforeAdd(RUN_TRANS entity)
        {
            entity.SEC_DEPTID = _userSession.ParentCompany.CorpID;
            entity.SEC_DEPT = _userSession.ParentCompany.CName;
            entity.DEPT_ID = _userSession.Corp.CorpID;
            entity.DEPT_NAME = _userSession.Corp.CName;
            entity.AUDITING = "0";
            entity.TRANS_ID = GuidHelper.NewSnowflakeId().ToString();
            await Task.CompletedTask;
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        public async Task<int> Submit(string sids)
        {
            return await _dbContext.UpdateAsync<RUN_TRANS>(x => x.TRANS_ID == sids,
                x => new RUN_TRANS
                {
                    AUDITING = "1",
                    SUBMITDATE =Sysdate,
                });
        }

        /// <summary>
        /// 获取运行状态一览表
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetAllRun(GridRequest request)
        {
            var detail = _dbContext.Query<RUN_TRANS>(a=>a.AUDITING=="1").Select(x => new
            {
                x.DEVICE_ID,
                x.SUBMITDATE,
            }).GroupBy(x => new
            {
                x.DEVICE_ID,
            }).Select(x => new
            {
                x.DEVICE_ID,
                SUBMITDATE = Sql.Max(x.SUBMITDATE),
            });
            //从 BC_CODE 取船机部的部门 ID
            var engineCorpId = (await _dbContext.Query<BC_CODE>(a => a.CODE_TYPE == "engineCorpId")
                .FirstAsync()).CODE_EN;
            //除超管和船机部外，按部门过滤数据
            var qry = _dbContext.Query<DEVICE_CARD>()
                 .WhereIf(!_userSession.IsAdmin && _userSession.Corp.CorpID != engineCorpId, a => _userSession.Corp.CorpID == a.DEPT_ID)
                 .LeftJoin(detail, (a, b) => a.DEVICE_ID == b.DEVICE_ID)
                 .LeftJoin<RUN_TRANS>((a, b, c) => b.DEVICE_ID == c.DEVICE_ID && b.SUBMITDATE == c.SUBMITDATE &&c.AUDITING=="1")
                 .LeftJoin<BC_CODE>((a, b, c, d) => d.CODE_EN == c.NEW_RUN_STATUS)
                 .Where((a, b, c, d) => a.AUDITING=="1"&&a.STATUS=="1"&&a.TYPE_ID=="2")
                 .Select((a, b, c, d) => new
                 {
                     NEW_RUN_STATUS = c.NEW_RUN_STATUS ?? "正常",
                     a.DEVICE_NO,
                     a.DEVICE_NAME,
                     a.TYPE_NAME,
                     a.SEC_DEPT,
                     a.DEPT_NAME,
                     b.SUBMITDATE,
                     c.TRANS_MEMO,
                     CODE_SEQ = d.CODE_SEQ ?? 10,
                 })
                .OrderBy(a => a.CODE_SEQ)
                .ThenBy(c => c.DEVICE_NO);

            return await qry.GetGridData(request);
        }
    }
}