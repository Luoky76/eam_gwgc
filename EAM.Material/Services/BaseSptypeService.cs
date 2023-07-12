using EAM.Material.Interfaces;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;

namespace EAM.Material.Services
{
    public class BaseSptypeService : BaseService, IBaseSptypeService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;

        public BaseSptypeService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
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
            return await _dbContext.Query<BASE_SPTYPE>().GetGridData(request);
        }

        /// <summary>
        /// 获取单行数据
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<AjaxResult> GetAsync(string id)
        {
            var row = await _dbContext.Query<BASE_SPTYPE>().Where(c => c.TYPE_ID == id).FirstAsync();
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
                    //{ "TerminalData", "码头" },
                    //{ "DeviceTypeData", "设备类型" }
                });
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
        public async Task<AjaxResult> Save(SaveRequest<BASE_SPTYPE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.TYPE_CODE,
                    c.TYPE_NAME,
                    c.TYPE_LEVEL,
                    c.IS_CANCEL,
                    c.MEMO,
                    c.TYPE_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE,
                    c.PURTYPE_ID,
                    c.PURTYPE_NAME,
                    c.EDIT_USER,
                    c.EDIT_DATE,
                    c.NOVALID_DATE
                },
                c => a => a.TYPE_ID == c.TYPE_ID, BeforeAdd, BeforeUpdate, null, false, null, AfterSave
                );
        }

        private async Task AfterSave(List<BASE_SPTYPE> adds, List<BASE_SPTYPE> updates, List<BASE_SPTYPE> deletes)
        {
            string operType = "物资分类";
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

        private async Task BeforeAdd(BASE_SPTYPE entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.TYPE_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.CREATE_USERID = _userSession.UserName;
            entity.CREATEDATE = dt;
            entity.MODIFY_USERID = _userSession.UserName;
            entity.MODIFYDATE = dt;
        }

        private async Task BeforeUpdate(BASE_SPTYPE entity)
        {
            DateTime? dt = await _dbContext.GetSysdate();

            entity.MODIFY_USERID = _userSession.UserName;
            entity.MODIFYDATE = dt;

        }
    }
}
