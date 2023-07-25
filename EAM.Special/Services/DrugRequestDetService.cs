using Chloe;
using EAM.Special.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Special.Services
{
    public class DrugRequestDetService : IDrugRequestDetService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;

        public DrugRequestDetService(IDbContext dbContext, IComboxDataService comboxDataService)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<DRUG_REQUEST_DET>().Select(c => new
            {
                c.REQUEST_DET_ID,
                c.REQUEST_ID,
                c.SP_ID,
                c.SP_STATUS,
                c.SP_CODE,
                c.SP_DAIMA,
                c.SP_NAME,
                c.SP_ENGNAME,
                c.SP_TYPE,
                c.SP_TUHAO,
                c.OTHER_CODE,
                c.BRAND,
                c.UNIT,
                c.FACTORY,
                c.REQUEST_NUM,
                c.MEMO,
                c.TYPE_ID,
                c.TYPE_NAME,
                c.TYPE_CODE,
                c.PURPOSE,
                c.CREATE_USERID,
                c.CREATEDATE,
                c.MODIFY_USERID,
                c.MODIFYDATE
            }).GetGridData(request);
            return list;
        }

        /// <summary>
        /// 根据ID获取单行记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DRUG_REQUEST_DET> GetAsync(string id)
        {
            var query = await _dbContext.Query<DRUG_REQUEST_DET>().Where(c => c.REQUEST_DET_ID == id).FirstAsync();
            return query;
        }

        /// <summary>
        /// 根据药品需求ID REQUEST_ID 获取多行记录
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public async Task<GridData> GetCertainRequestAsync(string requestId)
        {
            var query = await _dbContext.Query<DRUG_REQUEST_DET>()
                .Where(a => a.REQUEST_ID == requestId)
                .Select(c => new
                {
                    c.REQUEST_DET_ID,
                    c.REQUEST_ID,
                    c.SP_ID,
                    c.SP_STATUS,
                    c.SP_CODE,
                    c.SP_DAIMA,
                    c.SP_NAME,
                    c.SP_ENGNAME,
                    c.SP_TYPE,
                    c.SP_TUHAO,
                    c.OTHER_CODE,
                    c.BRAND,
                    c.UNIT,
                    c.FACTORY,
                    c.REQUEST_NUM,
                    c.MEMO,
                    c.TYPE_ID,
                    c.TYPE_NAME,
                    c.TYPE_CODE,
                    c.PURPOSE,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                }).GetGridData(null);
            return query;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAsync(SaveRequest<DRUG_REQUEST_DET> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.REQUEST_DET_ID,
                    c.REQUEST_ID,
                    c.SP_ID,
                    c.SP_STATUS,
                    c.SP_CODE,
                    c.SP_DAIMA,
                    c.SP_NAME,
                    c.SP_ENGNAME,
                    c.SP_TYPE,
                    c.SP_TUHAO,
                    c.OTHER_CODE,
                    c.BRAND,
                    c.UNIT,
                    c.FACTORY,
                    c.REQUEST_NUM,
                    c.MEMO,
                    c.TYPE_ID,
                    c.TYPE_NAME,
                    c.TYPE_CODE,
                    c.PURPOSE,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                },
                c => a => a.REQUEST_DET_ID == c.REQUEST_DET_ID
                , BeforeAdd, null, null, false, null, null);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(DRUG_REQUEST_DET entity)
        {
            if (entity.REQUEST_DET_ID.IsNullOrEmpty())
            {
                entity.REQUEST_DET_ID = GuidHelper.NewSnowflakeId().ToString();
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(DRUG_REQUEST_DET entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(DRUG_REQUEST_DET entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 保存后验证
        /// </summary>
        private async Task AfterSave(List<DRUG_REQUEST_DET> added, List<DRUG_REQUEST_DET> updated, List<DRUG_REQUEST_DET> deleted)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public async Task<AjaxResult> ComboxData()
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
