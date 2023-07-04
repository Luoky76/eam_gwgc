using Chloe;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Grid;
using Gksyb.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gksyb.Common;
using EAM.Device.Interfaces;

namespace EAM.Device.Services
{
    public class DeviceRemouldService : BaseService, IDeviceRemouldService
    {
        private readonly IDbContext _dbContext;
        private readonly UserSession _userSession;
        private readonly IComboxDataService _comboxService;
        private DateTime? _Sysdate;

        public DeviceRemouldService(IDbContext dbContext, UserSession userSession, IComboxDataService comboxService)
        {
            _dbContext = dbContext;
            _userSession = userSession;
            _comboxService = comboxService;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<DEVICE_CARD>().LeftJoin<DEVICE_REMOULD>((a,b) => a.DEVICE_ID == b.DEVICE_ID).Select((a,b) => new { 
                a.DEVICE_ID,
                a.DEVICE_CODE,
                a.DEVICE_NAME,
                a.DEVICE_TYPE,
                b.REMOULD_TYPE,
                b.DEVICE_REMOULD_ID,
                b.START_DATE,
                b.END_DATE,
                b.REMOULD_DESC,
                b.ISFINISH,
                b.REMARK
            }).GetGridData(request);
            return list;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAsync(SaveRequest<DEVICE_REMOULD> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.DEVICE_ID,
                    c.REMOULD_TYPE,
                    c.REMOULD_DESC,
                    c.ISFINISH,
                    c.REMARK,
                    c.START_DATE,
                    c.END_DATE,
                    c.DEVICE_REMOULD_ID,
                    c.ADD_USERID,
                    c.ADD_DATE,
                    c.MODIFY_USERID,
                    c.MODIFY_DATE,
                },
                c => a => a.DEVICE_REMOULD_ID == c.DEVICE_REMOULD_ID
                , BeforeAdd, BeforeUpdate, BeforeDelete, false, null, AfterSave);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(DEVICE_REMOULD entity)
        {
            entity.DEVICE_REMOULD_ID = GuidHelper.NewSnowflakeId().ToString();
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
        private async Task BeforeUpdate(DEVICE_REMOULD entity)
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
        private async Task BeforeDelete(DEVICE_REMOULD entity)
        {
            await Task.CompletedTask;
        }
        private async Task AfterSave(List<DEVICE_REMOULD> added, List<DEVICE_REMOULD> updated, List<DEVICE_REMOULD> deleted)
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
