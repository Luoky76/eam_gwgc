using Chloe;
using EAM.Device.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Device.Services
{
    public class DeviceCardService :  IDeviceCardService
    {
        private readonly IDbContext _dbContext;
        private readonly UserSession _userSession;
        private DateTime? _Sysdate;

        public DeviceCardService(IDbContext dbContext, UserSession userSession)
        {
            _dbContext = dbContext;
            _userSession = userSession;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<DEVICE_CARD>().GetGridData(request);
            return list;
        }

        public async Task<AjaxResult> SaveAsync(SaveRequest<DEVICE_CARD> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.DEVICE_ID,
                    c.DEVICE_NO,
                    c.DEVICE_NAME,
                    c.TYPE_NAME,
                    c.BOM_NAME,
                    c.DEVICE_TYPE,
                    c.SHIP_ID,
                    c.INSTALL_SITE,
                    c.SHIP_LENGTH,
                    c.SHIP_WIDTH,
                    c.SHIP_DEPTH,
                    c.SHIP_DRAFT,
                    c.SHIP_TYPE,
                    c.MAIN_POWER,
                    c.SECOND_POWER,
                    c.SPEED,
                    c.DRAG_FORCE,
                    c.TOTAL_TON,
                    c.PURE_TON,
                    c.REV_DATE,
                    c.AUDITING,
                    c.ADD_DATE,
                    c.DEPT_NAME,
                    c.WDEPT_NAME,
                    c.CARD_DATE,
                    c.STATUS,
                    c.PROVIDER_NAME,
                    c.OUT_CODE,
                    c.OUT_DATE,
                    c.GRAPH_NO,
                    c.PURPOSE,
                    c.FACTORY,
                    c.STATUS_DATE,
                    c.REP_DATE,
                    c.IS_GREEN,
                    c.ASSET_CODE,
                    c.DEP_MONTH,
                    c.DEP_VALUE,
                    c.PRICE,
                    c.ORG_VALUE,
                    c.NET_VALUE,
                    c.USE_YEAR,
                    c.NET_RATE,
                    c.DEP_RATE,
                    c.ASSET_SOURCE,
                    c.INSTALL_FEE,
                    c.PARAMS,
                    c.MEMO,
                    c.FINAN_TYPE,
                    c.REP_DATE5,
                    c.CARD_USER
                },
                c => a => a.DEVICE_ID == c.DEVICE_ID
                , BeforeAdd, BeforeUpdate, BeforeDelete);
        }

        /// <summary>
        /// 获取单行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DEVICE_CARD> GetAsync(string id)
        {
            return await _dbContext.Query<DEVICE_CARD>().Where(c => c.DEVICE_ID == id).FirstAsync();
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(DEVICE_CARD entity)
        {
            entity.DEVICE_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.ADD_DATE = Sysdate;
            entity.ADD_USERID = _userSession.UserID.ToString();
            entity.MODIFY_DATE = Sysdate;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(DEVICE_CARD entity)
        {
            entity.MODIFY_DATE = Sysdate;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.AUDITING = "1";

            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(DEVICE_CARD entity)
        {
            await Task.CompletedTask;
        }

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
    }
}