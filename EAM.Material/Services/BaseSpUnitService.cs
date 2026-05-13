using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Services
{
    public class BaseSpUnitService : BaseService, IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly UserSession _userSession;

        public BaseSpUnitService(IDbContext dbContext, UserSession userSession)
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
            return await _dbContext.Query<SP_UNIT>().GetGridData(request);
        }


        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> Save(SaveRequest<SP_UNIT> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.UNIT,
                    c.MEMO,
                    c.EDIT_USER,
                    c.EDIT_USERID,
                    c.EDIT_DATE,
                    c.UNIT_ID,
                    c.CREATE_USERID,
                    c.CREATE_DATE,
                    c.MODIFY_USERID,
                    c.MODIFY_DATE
                },
                c => a => a.UNIT_ID == c.UNIT_ID, BeforeAdd, BeforeUpdate);
        }

        /// <summary>
        /// 新增前处理
        /// </summary>
        private async Task BeforeAdd(SP_UNIT entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.UNIT_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = _userSession.UserID;
            entity.CREATE_DATE = dt;
            entity.MODIFY_USERID = _userSession.UserID;
            entity.MODIFY_USER = _userSession.RealName;
            entity.MODIFY_DATE = dt;
            entity.EDIT_USER = _userSession.RealName;
            entity.EDIT_USERID = _userSession.UserID.ToString();
            entity.EDIT_DATE = dt;
        }

        /// <summary>
        /// 更新前处理
        /// </summary>
        private async Task BeforeUpdate(SP_UNIT entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID;
            entity.MODIFY_DATE = dt;
            entity.MODIFY_USER = _userSession.RealName;
        }
    }
}
