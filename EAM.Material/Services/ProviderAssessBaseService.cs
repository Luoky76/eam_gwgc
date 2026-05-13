using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Services
{
    public class ProviderAssessBase : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;

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
        public async Task<PROVIDER_ASSESS_BASE> GetAsync(string id)
        {
            var query = await _dbContext.Query<PROVIDER_ASSESS_BASE>().Where(c => c.ASSESS_BASE_ID == id).FirstAsync();
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
                c.CREATE_USERID,
                c.CREATEDATE,
                c.MODIFY_USERID,
                c.MODIFYDATE
            }).GetGridData(request);
            return list;
        }

        /// <summary>
        /// 获取有效列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ValidListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<PROVIDER_ASSESS_BASE>()
                .Select(c => new
                {
                    c.ASSESS_BASE_ID,
                    c.IS_VALID,
                    c.CONTENT,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                })
                .Where(c => c.IS_VALID == "1")
                .GetGridData(request);
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
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                },
                c => a => a.ASSESS_BASE_ID == c.ASSESS_BASE_ID
                , BeforeAdd, null, null, false, null, null);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(PROVIDER_ASSESS_BASE entity)
        {
            if (entity.ASSESS_BASE_ID.IsNullOrEmpty())
            {
                entity.ASSESS_BASE_ID = GuidHelper.NewSnowflakeId().ToString();
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
        private async Task AfterSave(List<PROVIDER_ASSESS_BASE> added, List<PROVIDER_ASSESS_BASE> updated, List<PROVIDER_ASSESS_BASE> deleted)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public async Task<AjaxResult> ComboxDataAsync()
        {
            try
            {
                var data = await _comboxDataService.Get(new Dictionary<string, object>()
                {

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
