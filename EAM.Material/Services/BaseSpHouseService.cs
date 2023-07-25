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
    public class BaseSpHouseService : BaseService, IBaseSpHouseService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;

        public BaseSpHouseService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
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
            return await _dbContext.Query<SP_HOUSE>().GetGridData(request);
        }

        /// <summary>
        /// 获取单行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<AjaxResult> GetAsync(string id)
        {
            var row = await _dbContext.Query<SP_HOUSE>().Where(c => c.HOUSE_ID == id).FirstAsync();
            return AjaxResult.Success(row);
        }



        /// <summary>
        /// 获取下拉框信息
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ComboxData()
        {
            try
            {
                var dic = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "BCCode", "houseproperty" },
                    { "SpHouseName", (Expression<Func<SP_HOUSE, bool>>)null},
                    { "DeptData",(Expression<Func<CF_CORP, bool>>)(a => a.CORPID == _userSession.Corp.CorpID)},
                });

                var dic1 = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "BCCode", "housetype" }
                });
                dic.TryAdd("HouseType", dic1["BCCode"]);
                return AjaxResult.Success(dic);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }


        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> Save(SaveRequest<SP_HOUSE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING,
                    c.HOUSE_CODE,
                    c.HOUSE_NAME,
                    c.HOUSE_TYPE,
                    c.MEMO,
                    c.HOUSE_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.HOUSE_LEVEL,
                    c.IS_ACCOUNT,
                    c.EDIT_USER,
                    c.HOUSE_PROPERTY,
                    c.NOVALID_DATE,
                    c.CYCLE_DATE,
                    c.DEPT_ID,
                    c.DEPT_NAME,
                    c.IS_NOVALID,
                    c.EDIT_USERID,
                    c.SEC_DEPTID,
                    c.IS_SYNC,
                    c.SEC_DEPT,
                    c.PARENT_HOUSE_CODE
                },
                c => a => a.HOUSE_ID == c.HOUSE_ID, BeforeAdd, BeforeUpdate, null, false, null, AfterSave
                );
        }

        private async Task AfterSave(List<SP_HOUSE> adds, List<SP_HOUSE> updates, List<SP_HOUSE> deletes)
        {
            string operType = "仓库货位";
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

        private async Task BeforeAdd(SP_HOUSE entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.HOUSE_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.HOUSE_CODE = string.IsNullOrEmpty(entity.PARENT_HOUSE_CODE) ? entity.HOUSE_CODE : entity.PARENT_HOUSE_CODE + entity.HOUSE_CODE + "";
            entity.CREATE_USERID = _userSession.UserName;
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserName;
            entity.MODIFYDATE = dt;
        }

        private async Task BeforeUpdate(SP_HOUSE entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserName;
            entity.MODIFYDATE = dt;

        }

        /// <summary>
        /// 获取树形结构
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> TreeAsync()
        {
            var list = await _dbContext.Query<SP_HOUSE>().ToListAsync();
            var data = list.Select(c => new
            {
                c.HOUSE_CODE,
                c.HOUSE_NAME,
                c.HOUSE_ID,
                PARENTID = (string.IsNullOrWhiteSpace(c.PARENT_HOUSE_CODE) || c.PARENT_HOUSE_CODE == "0") ? "ROOT" : c.PARENT_HOUSE_CODE,
                ICON = "fa fa-group"
            }).OrderBy(c => c.HOUSE_CODE).ToList();
            data.Add(new { HOUSE_CODE = "ROOT", HOUSE_NAME = "仓库货位", HOUSE_ID = "ROOT", PARENTID = "", ICON = "fa fa-sitemap" });
            return AjaxResult.Success(data, "成功");
        }


        public async Task<int> Submit(List<string> sids)
        {
            var updatedevice = await _dbContext.UpdateAsync<SP_HOUSE>(x => sids.Contains(x.HOUSE_ID),
                    x => new SP_HOUSE
                    {
                        AUDITING = "1"
                    });
            return updatedevice;
        }
    }
}
