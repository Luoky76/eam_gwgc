using Chloe;
using EAM.Special.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Special.Services
{
    public class DrugCollectDetService : IDrugCollectDetService, IBaseService
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
                c.SP_NAME,
                c.SP_CODE,
                c.SP_SIZE,
                c.TYPE_ID,
                c.TYPE_NAME,
                c.TYPE_CODE,
                c.PRODUCE,
                c.UNIT,
                c.COLLECT_NUM,
                c.MEMO,
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
            var collectQuery = _dbContext.Query<DRUG_COLLECT>()
                .Where(a => a.AUDITING == "1")
                .LeftJoin<DRUG_COLLECT_REQUEST>((a, b) => a.COLLECT_ID == b.COLLECT_ID)
                .Select((a, b) => new
                {
                    b.SP_ID,
                    SUM_COLLECT_NUM = Sql.Sum(b.COLLECT_NUM)
                })
                .GroupBy(c => c.SP_ID)
                .Select(c => new
                {
                    c.SP_ID,
                    c.SUM_COLLECT_NUM
                });

            var requestQuery = _dbContext.Query<DRUG_REQUEST>()
                .Where(a => a.AUDITING == "1")
                .LeftJoin<DRUG_REQUEST_DET>((a, b) => a.REQUEST_ID == b.REQUEST_ID)
                .Select((a, b) => new
                {
                    b.SP_ID,
                    b.SP_NAME,
                    b.SP_CODE,
                    b.SP_SIZE,
                    b.TYPE_ID,
                    b.TYPE_NAME,
                    b.TYPE_CODE,
                    b.PRODUCE,
                    b.UNIT,
                    SUM_REQUEST_NUM = Sql.Sum(b.REQUEST_NUM)
                })
                .GroupBy(c => c.SP_ID)
                .AndBy(c => c.SP_NAME)
                .AndBy(c => c.SP_CODE)
                .AndBy(c => c.SP_SIZE)
                .AndBy(c => c.TYPE_ID)
                .AndBy(c => c.TYPE_NAME)
                .AndBy(c => c.TYPE_CODE)
                .AndBy(c => c.PRODUCE)
                .AndBy(c => c.UNIT)
                .Select(c => new
                {
                    c.SP_ID,
                    c.SP_NAME,
                    c.SP_CODE,
                    c.SP_SIZE,
                    c.TYPE_ID,
                    c.TYPE_NAME,
                    c.TYPE_CODE,
                    c.PRODUCE,
                    c.UNIT,
                    c.SUM_REQUEST_NUM
                });

            var list = await requestQuery.LeftJoin(collectQuery, (a, b) => a.SP_ID == b.SP_ID)
                .Select((a, b) => new
                {
                    a.SP_ID,
                    a.SP_NAME,
                    a.SP_CODE,
                    a.SP_SIZE,
                    a.TYPE_ID,
                    a.TYPE_NAME,
                    a.TYPE_CODE,
                    a.PRODUCE,
                    a.UNIT,
                    SUM_REQUEST_NUM = a.SUM_REQUEST_NUM - (b.SUM_COLLECT_NUM ?? 0)
                })
                .Where(c => c.SUM_REQUEST_NUM > 0)
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
                    c.SP_NAME,
                    c.SP_CODE,
                    c.SP_SIZE,
                    c.TYPE_ID,
                    c.TYPE_NAME,
                    c.TYPE_CODE,
                    c.PRODUCE,
                    c.UNIT,
                    c.COLLECT_NUM,
                    c.MEMO,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                }).GetGridData(null);
            return list;
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
                    c.SP_NAME,
                    c.SP_CODE,
                    c.SP_SIZE,
                    c.TYPE_ID,
                    c.TYPE_NAME,
                    c.TYPE_CODE,
                    c.PRODUCE,
                    c.UNIT,
                    c.COLLECT_NUM,
                    c.MEMO,
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
        public async Task<AjaxResult> ComboxDataAsync()
        {
            try
            {
                var data = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "Auditing", null },
                    { "User", null },
                    { "DrugCollectMethod", null },
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
