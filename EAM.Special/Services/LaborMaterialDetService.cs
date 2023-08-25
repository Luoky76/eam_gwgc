using Chloe;
using EAM.Special.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.IdentityModel.Tokens;

namespace EAM.Special.Services
{
    public class LaborMaterialDetService : ILaborMaterialDetService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;

        public LaborMaterialDetService(IDbContext dbContext, IComboxDataService comboxDataService)
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
            var list = await _dbContext.Query<LABOR_MATERIAL_DET>()
                .GetGridData(request);
            return list;
        }

        /// <summary>
        /// 根据常规物料主表ID LABOR_MATERIAL_ID 获取列表
        /// </summary>
        /// <param name="laborMaterialId"></param>
        /// <returns></returns>
        public async Task<GridData> GetCertainLaborMaterialAsync(string laborMaterialId)
        {
            var list = await _dbContext.Query<LABOR_MATERIAL_DET>()
                .Where(c => c.LABOR_MATERIAL_ID == laborMaterialId)
                .GetGridData(null);
            return list;
        }

        /// <summary>
        /// 根据ID获取单行记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<LABOR_MATERIAL_DET> GetAsync(string id)
        {
            var query = await _dbContext.Query<LABOR_MATERIAL_DET>()
                .Where(c => c.LABOR_MATERIAL_DET_ID == id)
                .FirstAsync();
            return query;
        }

        /// <summary>
        /// 生成主键
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public string CreatePrimaryKey()
        {
            return GuidHelper.NewSnowflakeId().ToString();
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAsync(SaveRequest<LABOR_MATERIAL_DET> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.LABOR_MATERIAL_DET_ID,
                    c.LABOR_MATERIAL_ID,
                    c.SP_ID,
                    c.SP_NAME,
                    c.SP_CODE,
                    c.SP_SIZE,
                    c.TYPE_ID,
                    c.TYPE_NAME,
                    c.TYPE_CODE,
                    c.UNIT,
                    c.STORE_NUM,
                    c.POSITION,
                    c.MEMO,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                },
                c => a => a.LABOR_MATERIAL_DET_ID == c.LABOR_MATERIAL_DET_ID
                , BeforeAdd, null, null, false, null, null);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(LABOR_MATERIAL_DET entity)
        {
            if (entity.LABOR_MATERIAL_DET_ID.IsNullOrEmpty())
            {
                entity.LABOR_MATERIAL_DET_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(LABOR_MATERIAL_DET entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(LABOR_MATERIAL_DET entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 保存后验证
        /// </summary>
        private async Task AfterSave(List<LABOR_MATERIAL_DET> added, List<LABOR_MATERIAL_DET> updated, List<LABOR_MATERIAL_DET> deleted)
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
                    { "Auditing", null },
                    { "LaborMaterialCard", null }
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