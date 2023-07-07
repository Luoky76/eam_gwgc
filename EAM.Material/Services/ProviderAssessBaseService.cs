using EAM.Material.Interfaces;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Services
{
    public class ProviderAssessBase : BaseService, IProviderAssessBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;
        private DateTime? _Sysdate;

        public ProviderAssessBase(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userSession = userSession;
        }

        /// <summary>
        /// 根据ID获取单行记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<PROVIDER_ASSESS_BASE> GetAsync(object id)
        {
            string? sid = id.ToString();
            var query = await _dbContext.Query<PROVIDER_ASSESS_BASE>().Where(c => c.ASSESS_BASE_ID == sid).FirstAsync();
            return query;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<PROVIDER_ASSESS_BASE>().Select(c => new
            {
                c.ASSESS_BASE_ID,
                c.IS_VALID,
                c.CONTENT,
                c.ADD_USERID,
                c.ADD_DATE,
                c.MODIFY_USERID,
                c.MODIFY_DATE,
                c.TENANT_ID
            }).GetGridData(request);
            return list;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAsync(SaveRequest<PROVIDER_ASSESS_BASE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.ASSESS_BASE_ID,
                    c.IS_VALID,
                    c.CONTENT,
                    c.ADD_USERID,
                    c.ADD_DATE,
                    c.MODIFY_USERID,
                    c.MODIFY_DATE,
                    c.TENANT_ID
                },
                c => a => a.ASSESS_BASE_ID == c.ASSESS_BASE_ID
                , BeforeAdd, BeforeUpdate, BeforeDelete, false, null, AfterSave);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(PROVIDER_ASSESS_BASE entity)
        {
            entity.ASSESS_BASE_ID = GuidHelper.NewSnowflakeId().ToString();

            if (string.IsNullOrEmpty(entity.ASSESS_BASE_ID))
            {
                entity.ASSESS_BASE_ID = _userSession.Corp.CorpID;
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(PROVIDER_ASSESS_BASE entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(PROVIDER_ASSESS_BASE entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 保存后验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task AfterSave(List<PROVIDER_ASSESS_BASE> added, List<PROVIDER_ASSESS_BASE> updated, List<PROVIDER_ASSESS_BASE> deleted)
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

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public async Task<AjaxResult> ComboxData()
        {
            try
            {
                var data = await _comboxDataService.Get(new Dictionary<string, object>(){
                    
                });

                return AjaxResult.Success(data);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }
    }
}
