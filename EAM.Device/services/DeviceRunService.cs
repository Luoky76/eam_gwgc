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

namespace EAM.Device.services
{
    public class DeviceRunService : IDeviceRunService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxService;
        private readonly UserSession _userSession;

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
            var detail = _dbContext.Query<RUN_TRANS>().Select(x => new
            {
                x.DEVICE_ID,
                x.ADD_DATE,
            }).GroupBy(x => new
            {
                x.DEVICE_ID,
            }).Select(x => new
            {
                x.DEVICE_ID,
                ADD_DATE = Sql.Max(x.ADD_DATE),
            });

            var qry = _dbContext.Query<DEVICE_CARD>()
                .WhereIf(!_userSession.IsAdmin, a => _userSession.Corp.CorpID == a.SEC_DEPTID)
                .LeftJoin(detail, (a, b) => a.DEVICE_ID == b.DEVICE_ID)
                .LeftJoin<RUN_TRANS>((a, b, c) => b.DEVICE_ID == c.DEVICE_ID && b.ADD_DATE == c.ADD_DATE)
                .Where((a, b, c) => a.AUDITING=="1"&&a.STATUS=="在用"&&c.AUDITING=="1");
            return await qry
                .Select((a, b, c) => new ComboxData()
                {
                    ID = a.DEVICE_ID,
                    TEXT = a.DEVICE_NAME,
                    VALUE = a.DEVICE_NO,
                    EXTEND =c.NEW_RUN_STATUS,
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
            var qry = _dbContext.Query<RUN_TRANS>();
            if (!_userSession.IsAdmin)
            {
                qry = qry.Where(c => _userSession.Corp.CorpID == c.SEC_DEPTID)
                    .OrderByDesc(c => c.AUDITING)
                    .ThenByDesc(c => c.TRANS_DATE);
            }
            else
            {
                qry = qry.OrderByDesc(c => c.AUDITING)
                        .ThenByDesc(c => c.TRANS_DATE);
            }
            return await qry.GetGridData(request);
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

            entity.SEC_DEPTID = _userSession.Corp.CorpID;
            entity.SEC_DEPT = _userSession.Corp.CName;
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
        public async Task<AjaxResult> Submit(string sids)
        {
            var changeAuditing = await _dbContext.UpdateAsync<RUN_TRANS>(x => x.TRANS_ID == sids,
                x => new RUN_TRANS
                {
                    AUDITING = "1",
                });
            return AjaxResult.Success("成功");
        }

        /// <summary>
        /// 获取运行状态一览表
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetAllRun(GridRequest request)
        {
            var detail = _dbContext.Query<RUN_TRANS>().Select(x => new
            {
                x.DEVICE_ID,
                x.ADD_DATE,
            }).GroupBy(x => new
            {
                x.DEVICE_ID,
            }).Select(x => new
            {
                x.DEVICE_ID,
                ADD_DATE = Sql.Max(x.ADD_DATE),
            });

            var qry = _dbContext.Query<DEVICE_CARD>()
                 .WhereIf(!_userSession.IsAdmin, a => _userSession.Corp.CorpID == a.SEC_DEPTID)
                 .LeftJoin(detail, (a, b) => a.DEVICE_ID == b.DEVICE_ID)
                 .LeftJoin<RUN_TRANS>((a, b, c) => b.DEVICE_ID == c.DEVICE_ID && b.ADD_DATE == c.ADD_DATE)
                 .LeftJoin<BC_CODE>((a, b, c, d) => d.CODE_EN == c.NEW_RUN_STATUS)
                 .Where((a, b, c, d) => a.AUDITING=="1"&&a.STATUS=="在用"&&c.AUDITING=="1")
                 .Select((a, b, c, d) => new
                 {
                     NEW_RUN_STATUS = c.NEW_RUN_STATUS ?? "正常",
                     a.DEVICE_NO,
                     a.DEVICE_NAME,
                     a.TYPE_NAME,
                     a.SEC_DEPT,
                     a.DEPT_NAME,
                     b.ADD_DATE,
                     c.TRANS_MEMO,
                     CODE_SEQ = d.CODE_SEQ ?? 10,
                 })
                .OrderBy(a => a.CODE_SEQ)
                .ThenBy(c => c.DEVICE_NO);

            return await qry.GetGridData(request);

        }
    }
}