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
    public class RepInfoService : IRepInfoService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxService;
        private readonly UserSession _userSession;

        public RepInfoService(IDbContext dbContext, IComboxDataService comboxService, UserSession userSession)
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
                { "FaultSrc",null},
                { "FrdbLevel",null},
                { "ShipInfo",null},
            });
        }

        /// <summary>
        /// 获取故障库记录
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetRepFrdbList(GridRequest request)
        {
            return await _dbContext.Query<REP_FRDB>()
                .OrderBy(c => c.AUDITING)
                .ThenByDesc(c => c.FRDB_CODE)
                .GetGridData(request);
        }

        /// <summary>
        /// 获取单条故障库记录
        /// </summary>
        /// <returns></returns>

        public async Task<REP_FRDB> GetRepFrdbListDetail(string ID)
        {
            var qry = await _dbContext.QueryByKeyAsync<REP_FRDB>(ID);
            return qry;
        }

        /// <summary>
        /// 管理故障库记录
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManageRepFrdb(SaveRequest<REP_FRDB> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING,
                    c.FRDB_CODE,
                    c.FRDB_LEVEL,
                    c.DEVICE_ID,
                    c.DEVICE_NAME,
                    c.DEVICE_CODE,
                    c.REP_DEVICE,
                    c.SRC_FUNCTION,
                    c.SRC_CODE,
                    c.EDIT_USER,
                    c.MEMO,
                    c.EDIT_USERID,
                    c.EDIT_DATE,
                    c.FAULT_DESCRIBE,
                    c.FAULT_REASON,
                    c.MEASURES,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.FRDB_ID,
                },
                c => a => a.FRDB_ID  == c.FRDB_ID, BeforeAdd);
        }

        public async Task BeforeAdd(REP_FRDB entity)
        {
            entity.EDIT_USER = _userSession.RealName;
            entity.EDIT_USERID = _userSession.UserID.ToString();
            entity.EDIT_DATE = await _dbContext.GetSysdate();
            string aa = "GZK" + DateTime.Now.ToString("yyyyMM");
            string def = aa + "0000";
            var model = await _dbContext.Query<REP_FRDB>(x => x.FRDB_CODE.Contains(aa)).Select(x => Sql.Max(x.FRDB_CODE) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(9, 4).CastTo<int>() + 1;
            entity.FRDB_CODE = aa + index.ToString("D4");
            entity.AUDITING = "0";
            entity.FRDB_ID = GuidHelper.NewSnowflakeId().ToString();
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        public async Task<int> Submit(List<string> sids)
        {
            return await _dbContext.UpdateAsync<REP_FRDB>(x => sids.Contains(x.FRDB_ID),
                x => new REP_FRDB
                {
                    AUDITING = "1",
                });
        }

        /// <summary>
        /// 获取故障分类
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> GetRepTypeList(GridRequest request)
        {
            return await _dbContext.Query<REP_TYPE>()
                .GetGridData(request);
        }

        /// <summary>
        /// 管理故障分类
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ManageRepType(SaveRequest<REP_TYPE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.MEMO,
                    c.REP_TYPE_NAME,
                    c.EDIT_USER,
                    c.EDIT_DATE,
                    c.REP_TYPE_ID,
                },
                c => a => a.REP_TYPE_ID == c.REP_TYPE_ID, BeforeAdd);
        }

        public async Task BeforeAdd(REP_TYPE entity)
        {
            entity.EDIT_USER = _userSession.RealName;
            entity.EDIT_USERID = _userSession.UserID.ToString();
            entity.EDIT_DATE = await _dbContext.GetSysdate();
            entity.REP_TYPE_ID = GuidHelper.NewSnowflakeId().ToString();
            await Task.CompletedTask;
        }
    }
}