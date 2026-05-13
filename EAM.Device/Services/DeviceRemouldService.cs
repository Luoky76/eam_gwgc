using Chloe;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Device.Services
{
    public class DeviceRemouldService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly UserSession _userSession;
        private DateTime? _Sysdate;

        public DeviceRemouldService(IDbContext dbContext, UserSession userSession)
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
            var list = await _dbContext.Query<DEVICE_REMOULD>().GetGridData(request);
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
                    c.DEVICE_REMOULD_ID
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

            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(DEVICE_REMOULD entity)
        {
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

        /// <summary>
        /// 保存后处理
        /// </summary>
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