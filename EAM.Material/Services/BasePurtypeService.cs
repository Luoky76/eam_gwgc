using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Services
{
    public class BasePurtypeService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly UserSession _userSession;

        public BasePurtypeService(IDbContext dbContext, UserSession userSession)
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
            return await _dbContext.Query<BASE_PURTYPE>().GetGridData(request);
        }


        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> Save(SaveRequest<BASE_PURTYPE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.PURTYPE_CODE,
                    c.PURTYPE_NAME,
                    c.MEMO,
                    c.PURTYPE_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                },
                c => a => a.PURTYPE_ID == c.PURTYPE_ID, BeforeAdd, BeforeUpdate);
        }

        /// <summary>
        /// 新增前处理
        /// </summary>
        private async Task BeforeAdd(BASE_PURTYPE entity)
        {
            entity.PURTYPE_ID = GuidHelper.NewSnowflakeId().ToString();
        }

        /// <summary>
        /// 更新前处理
        /// </summary>
        private async Task BeforeUpdate(BASE_PURTYPE entity)
        {
        }
    }
}
