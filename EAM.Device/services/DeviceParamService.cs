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
    public class DeviceParamService : BaseService, IDeviceParamService
    {
        private readonly IDbContext _dbContext;
        private readonly UserSession _userSession;
        private DateTime? _Sysdate;

        public DeviceParamService(IDbContext dbContext, UserSession userSession)
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
            var list = await _dbContext.Query<DEVICE_CARD>().LeftJoin<DEVICE_PARAM>((a, b) => a.DEVICE_ID == b.DEVICE_ID).Select((a, b) => new
            {
                a.DEVICE_ID,
                a.DEVICE_NO,
                b.PARAM_NAME,
                b.PARAM_VALUE,
                b.PARAM_MEMO,
                b.PARAM_CODE,
                b.PARAM_SUB,
                b.TECHFUN_NAME,
                b.PARAM_ID,
                b.ADD_USERID,
                b.ADD_DATE,
                b.MODIFY_USERID,
                b.MODIFY_DATE
            }).GetGridData(request);
            return list;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAsync(SaveRequest<DEVICE_PARAM> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.DEVICE_ID,
                    c.PARAM_NAME,
                    c.PARAM_VALUE,
                    c.PARAM_MEMO,
                    c.PARAM_CODE,
                    c.PARAM_SUB,
                    c.TECHFUN_NAME,
                    c.PARAM_ID,
                    c.ADD_USERID,
                    c.ADD_DATE,
                    c.MODIFY_USERID,
                    c.MODIFY_DATE,
                },
                c => a => a.PARAM_ID == c.PARAM_ID
                , BeforeAdd, BeforeUpdate, BeforeDelete, false, null, AfterSave);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(DEVICE_PARAM entity)
        {
            entity.DEVICE_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.PARAM_ID = GuidHelper.NewSnowflakeId().ToString();
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
        private async Task BeforeUpdate(DEVICE_PARAM entity)
        {
            entity.MODIFY_DATE = Sysdate;
            entity.MODIFY_USERID = _userSession.UserID.ToString();

            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(DEVICE_PARAM entity)
        {
            await Task.CompletedTask;
        }

        private async Task AfterSave(List<DEVICE_PARAM> added, List<DEVICE_PARAM> updated, List<DEVICE_PARAM> deleted)
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