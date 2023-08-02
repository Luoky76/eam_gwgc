using Chloe;
using EAM.Special.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Special.Services
{
    public class DrugCollectDetService : IDrugCollectDetService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly IUserService _userService;

        public DrugCollectDetService(IDbContext dbContext, IComboxDataService comboxDataService, IUserService userService)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userService = userService;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<DRUG_COLLECT_DET>().Select(c => new
            {
                c.COLLECT_DET_ID,
                c.COLLECT_ID,
                c.SP_ID,
                c.REQUEST_DET_ID,
                c.SP_CODE,
                c.SP_NAME,
                c.SP_TYPE,
                c.SP_DAIMA,
                c.SP_TUHAO,
                c.SP_ENGNAME,
                c.OTHER_CODE,
                c.BRAND,
                c.UNIT,
                c.FACTORY,
                c.COLLECT_NUM,
                c.STORE_NUM,
                c.MEMO,
                c.ARRIVE_NUM,
                c.IN_NUM,
                c.TYPE_CODE,
                c.TYPE_NAME,
                c.TYPE_ID,
                c.CREATE_USERID,
                c.CREATEDATE,
                c.MODIFY_USERID,
                c.MODIFYDATE
            }).GetGridData(request);
            return list;
        }

        /// <summary>
        /// 获取导入列表
        /// 包含尚未采购的药品SP_ID及总计所需数量
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ImportListAsync(GridRequest request)
        {
            var query = _dbContext.Query<DRUG_COLLECT>()
                .Where(a => a.AUDITING == "1")
                .LeftJoin<DRUG_COLLECT_REQUEST>((a, b) => a.COLLECT_ID == b.COLLECT_ID)
                .Select((a, b) => new
                {
                    b.REQUEST_DET_ID,
                    b.IS_FULLBUY,
                    b.COLLECT_NUM
                });

            var list = await _dbContext.Query<DRUG_REQUEST>()
                .Where(a => a.AUDITING == "1")
                .LeftJoin<DRUG_REQUEST_DET>((a, b) => a.REQUEST_ID == b.REQUEST_ID)
                .LeftJoin(query, (a, b, c) => b.REQUEST_DET_ID == c.REQUEST_DET_ID)
                .Where((a, b, c) => c.IS_FULLBUY == "0" || c.IS_FULLBUY == null)
                .Select((a, b, c) => new
                {
                    b.SP_ID,
                    b.SP_NAME,
                    b.SP_CODE,
                    b.SP_TYPE,
                    b.FACTORY,
                    b.UNIT,
                    SUM_REQUEST_NUM = Sql.Sum(b.REQUEST_NUM - (c.COLLECT_NUM == null ? 0 : c.COLLECT_NUM))
                })
                .GroupBy(e => e.SP_ID)
                .AndBy(e => e.SP_NAME)
                .AndBy(e => e.SP_CODE)
                .AndBy(e => e.SP_TYPE)
                .AndBy(e => e.FACTORY)
                .AndBy(e => e.UNIT)
                .Select(e => new
                {
                    e.SP_ID,
                    e.SP_NAME,
                    e.SP_CODE,
                    e.SP_TYPE,
                    e.FACTORY,
                    e.UNIT,
                    e.SUM_REQUEST_NUM
                })
                .GetGridData(request);
            return list;
        }

        /// <summary>
        /// 根据ID获取单行记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DRUG_COLLECT_DET> GetAsync(string id)
        {
            var query = await _dbContext.Query<DRUG_COLLECT_DET>().Where(c => c.COLLECT_DET_ID == id).FirstAsync();
            return query;
        }

        /// <summary>
        /// 根据COLLECT_ID获取列表
        /// </summary>
        /// <param name="collectId"></param>
        /// <returns></returns>
        public async Task<GridData> GetCertainCollectIdAsync(string collectId)
        {
            var list = await _dbContext.Query<DRUG_COLLECT_DET>()
                .Where(c => c.COLLECT_ID == collectId)
                .Select(c => new
                {
                    c.COLLECT_DET_ID,
                    c.COLLECT_ID,
                    c.SP_ID,
                    c.REQUEST_DET_ID,
                    c.SP_CODE,
                    c.SP_NAME,
                    c.SP_TYPE,
                    c.SP_DAIMA,
                    c.SP_TUHAO,
                    c.SP_ENGNAME,
                    c.OTHER_CODE,
                    c.BRAND,
                    c.UNIT,
                    c.FACTORY,
                    c.COLLECT_NUM,
                    c.STORE_NUM,
                    c.MEMO,
                    c.ARRIVE_NUM,
                    c.IN_NUM,
                    c.TYPE_CODE,
                    c.TYPE_NAME,
                    c.TYPE_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                }).GetGridData(null);
            return list;
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
        public async Task<AjaxResult> SaveAsync(SaveRequest<DRUG_COLLECT_DET> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.COLLECT_DET_ID,
                    c.COLLECT_ID,
                    c.SP_ID,
                    c.REQUEST_DET_ID,
                    c.SP_CODE,
                    c.SP_NAME,
                    c.SP_TYPE,
                    c.SP_DAIMA,
                    c.SP_TUHAO,
                    c.SP_ENGNAME,
                    c.OTHER_CODE,
                    c.BRAND,
                    c.UNIT,
                    c.FACTORY,
                    c.COLLECT_NUM,
                    c.STORE_NUM,
                    c.MEMO,
                    c.ARRIVE_NUM,
                    c.IN_NUM,
                    c.TYPE_CODE,
                    c.TYPE_NAME,
                    c.TYPE_ID,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                },
                c => a => a.COLLECT_DET_ID == c.COLLECT_DET_ID
                , BeforeAdd, null, null, false, null, null);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(DRUG_COLLECT_DET entity)
        {
            if (entity.COLLECT_DET_ID.IsNullOrEmpty())
            {
                entity.COLLECT_DET_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(DRUG_COLLECT_DET entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(DRUG_COLLECT_DET entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 保存后验证
        /// </summary>
        private async Task AfterSave(List<DRUG_COLLECT_DET> added, List<DRUG_COLLECT_DET> updated, List<DRUG_COLLECT_DET> deleted)
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
                    { "User", null }
                });
                //data.TryAdd("User", await _userService.ComboxDataAsync());
                return AjaxResult.Success(data);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }
    }
}
