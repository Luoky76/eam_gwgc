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
    public class BaseSpUnitService : BaseService, IBaseSpUnitService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;

        public BaseSpUnitService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
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
                c => a => a.UNIT_ID == c.UNIT_ID, BeforeAdd, BeforeUpdate, null, false, null, AfterSave
                );
        }

        private async Task AfterSave(List<SP_UNIT> adds, List<SP_UNIT> updates, List<SP_UNIT> deletes)
        {
            string operType = "计量单位";
            foreach (var entity in adds)
            {
                await _dbContext.DBLog(operType + Oper.Add, entity, null, _userSession.RealName);
            }
            foreach (var entity in updates)
            {
                await _dbContext.DBLog(operType + Oper.Modify, entity, null, _userSession.RealName);
            }
            foreach (var entity in deletes)
            {
                await _dbContext.DBLog(operType + Oper.Delete, entity, null, _userSession.RealName);
            }
        }

        private async Task BeforeAdd(SP_UNIT entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.UNIT_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = _userSession.UserID;
            entity.CREATE_DATE = dt;
            entity.MODIFY_USERID = _userSession.UserID;
            entity.MODIFY_DATE = dt;
            entity.EDIT_USER = _userSession.RealName;
            entity.EDIT_USERID = _userSession.UserID.ToString();
            entity.EDIT_DATE = dt;
        }

        private async Task BeforeUpdate(SP_UNIT entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserID;
            entity.MODIFY_DATE = dt;

        }
    }
}
