using DocumentFormat.OpenXml.Drawing.Charts;
using EAM.Material.Interfaces;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace EAM.Material.Services
{
    public class BasePurtypeService : BaseService, IBasePurtypeService
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

        private async Task BeforeAdd(BASE_PURTYPE entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.PURTYPE_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = _userSession.UserID.ToString();
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;
        }

        private async Task BeforeUpdate(BASE_PURTYPE entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = dt;

        }
    }
}
