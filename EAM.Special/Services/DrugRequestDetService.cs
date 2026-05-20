using Chloe;
using Gksyb.Common;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Special.Services
{
    public class DrugRequestDetService : IBaseService
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
                c.SP_NAME,
                c.SP_CODE,
                c.SP_SIZE,
                c.TYPE_ID,
                c.TYPE_NAME,
                c.TYPE_CODE,
                c.PRODUCE,
                c.UNIT,
                c.REQUEST_NUM,
                c.MEMO,
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
            //获取剩余药品数量
            var list = await _dbContext.Query<DRUG_REQUEST>()
                .Where(a => a.REQUEST_ID == requestId)
                .Select(a => new
                {
                    a.REQUEST_TYPE,
                    a.REQUEST_YEAR,
                    a.REQUEST_MONTH,
                    a.DEPT_ID,
                    a.POSITION
                })
                .ToListAsync();

            if (!list.Any())
            {
                return await _dbContext.Query<DRUG_REQUEST_DET>()
                .Where(a => a.REQUEST_ID == requestId)
                .GetGridData(null);
            }

            string type = list[0].REQUEST_TYPE;
            int year = list[0].REQUEST_YEAR.Value;
            int month = list[0].REQUEST_MONTH.Value;
            string deptID = list[0].DEPT_ID;
            //"1"港内，"2"港外
            string position = list[0].POSITION;

            if (month >= 4 && month <= 9)
            {
                var query = _dbContext.Query<DRUG_REQUEST>()
                    //已提交 需求类型为月度或厂修（不计入临时） 排除本需求单 相同部门 相同位置（港内或港外） 相同年份 4~9月
                    .Where(a => a.AUDITING == "1" && (a.REQUEST_TYPE == "1" || a.REQUEST_TYPE == "2")
                    && a.REQUEST_ID != requestId && a.DEPT_ID == deptID && a.POSITION == position
                    && a.REQUEST_YEAR == year && a.REQUEST_MONTH >= 4 && a.REQUEST_MONTH <= 9)
                    .LeftJoin<DRUG_REQUEST_DET>((a, b) => a.REQUEST_ID == b.REQUEST_ID)
                    .Select((a, b) => new
                    {
                        b.SP_ID,
                        SUM_REQUEST_NUM = Sql.Sum(b.REQUEST_NUM)
                    })
                    .GroupBy(c => c.SP_ID)
                    .Select(c => new
                    {
                        c.SP_ID,
                        SUM_REQUEST_NUM = c.SUM_REQUEST_NUM == null ? 0 : c.SUM_REQUEST_NUM
                    });

                var limitList = _dbContext.Query<DRUG_LIMIT>()
                .LeftJoin(query, (a, b) => a.SP_ID == b.SP_ID)
                .Select((a, b) => new
                {
                    a.SP_ID,
                    LEFTOVER = position == "1" ? a.INSIDE_APRIL - (b.SUM_REQUEST_NUM == null ? 0 : b.SUM_REQUEST_NUM) :
                    a.OUTSIDE_APRIL - (b.SUM_REQUEST_NUM == null ? 0 : b.SUM_REQUEST_NUM)
                });

                var requestDet = await _dbContext.Query<DRUG_REQUEST_DET>()
                .Where(a => a.REQUEST_ID == requestId)
                .LeftJoin(limitList, (a, b) => a.SP_ID == b.SP_ID)
                .Select((a, b) => new
                {
                    a.REQUEST_DET_ID,
                    a.REQUEST_ID,
                    a.SP_ID,
                    a.SP_NAME,
                    a.SP_CODE,
                    a.SP_SIZE,
                    a.TYPE_ID,
                    a.TYPE_NAME,
                    a.TYPE_CODE,
                    a.PRODUCE,
                    a.UNIT,
                    a.REQUEST_NUM,
                    a.MEMO,
                    a.PURPOSE,
                    a.CREATE_USERID,
                    a.CREATEDATE,
                    a.MODIFY_USERID,
                    a.MODIFYDATE,
                    b.LEFTOVER
                })
                .GetGridData(null);
                return requestDet;
            }
            else if (month <= 3)
            {
                var query = _dbContext.Query<DRUG_REQUEST>()
                    //已提交 需求类型为月度或厂修（不计入临时） 排除本需求单 相同部门 相同位置（港内或港外） 同年1~3月或去年10~12月
                    .Where(a => a.AUDITING == "1" && (a.REQUEST_TYPE == "1" || a.REQUEST_TYPE == "2")
                    && a.REQUEST_ID != requestId && a.DEPT_ID == deptID && a.POSITION == position
                    && (a.REQUEST_YEAR == year && a.REQUEST_MONTH <= 3 || a.REQUEST_YEAR == year - 1 && a.REQUEST_MONTH >= 10))
                    .LeftJoin<DRUG_REQUEST_DET>((a, b) => a.REQUEST_ID == b.REQUEST_ID)
                    .Select((a, b) => new
                    {
                        b.SP_ID,
                        SUM_REQUEST_NUM = Sql.Sum(b.REQUEST_NUM)
                    })
                    .GroupBy(c => c.SP_ID)
                    .Select(c => new
                    {
                        c.SP_ID,
                        SUM_REQUEST_NUM = c.SUM_REQUEST_NUM == null ? 0 : c.SUM_REQUEST_NUM
                    });

                var limitList = _dbContext.Query<DRUG_LIMIT>()
                .LeftJoin(query, (a, b) => a.SP_ID == b.SP_ID)
                .Select((a, b) => new
                {
                    a.SP_ID,
                    LEFTOVER = position == "1" ? a.INSIDE_OCTOBER - (b.SUM_REQUEST_NUM == null ? 0 : b.SUM_REQUEST_NUM) :
                    a.OUTSIDE_OCTOBER - (b.SUM_REQUEST_NUM == null ? 0 : b.SUM_REQUEST_NUM)
                });

                var requestDet = await _dbContext.Query<DRUG_REQUEST_DET>()
                .Where(a => a.REQUEST_ID == requestId)
                .LeftJoin(limitList, (a, b) => a.SP_ID == b.SP_ID)
                .Select((a, b) => new
                {
                    a.REQUEST_DET_ID,
                    a.REQUEST_ID,
                    a.SP_ID,
                    a.SP_NAME,
                    a.SP_CODE,
                    a.SP_SIZE,
                    a.TYPE_ID,
                    a.TYPE_NAME,
                    a.TYPE_CODE,
                    a.PRODUCE,
                    a.UNIT,
                    a.REQUEST_NUM,
                    a.MEMO,
                    a.PURPOSE,
                    a.CREATE_USERID,
                    a.CREATEDATE,
                    a.MODIFY_USERID,
                    a.MODIFYDATE,
                    b.LEFTOVER
                })
                .GetGridData(null);
                return requestDet;
            }
            else
            {
                var query = _dbContext.Query<DRUG_REQUEST>()
                    //已提交 需求类型为月度或厂修（不计入临时） 排除本需求单 相同部门 相同位置（港内或港外） 同年10~12月或下一年1~3月
                    .Where(a => a.AUDITING == "1" && (a.REQUEST_TYPE == "1" || a.REQUEST_TYPE == "2")
                    && a.REQUEST_ID != requestId && a.DEPT_ID == deptID && a.POSITION == position
                    && (a.REQUEST_YEAR == year && a.REQUEST_MONTH >= 10 || a.REQUEST_YEAR == year + 1 && a.REQUEST_MONTH <= 3))
                    .LeftJoin<DRUG_REQUEST_DET>((a, b) => a.REQUEST_ID == b.REQUEST_ID)
                    .Select((a, b) => new
                    {
                        b.SP_ID,
                        SUM_REQUEST_NUM = Sql.Sum(b.REQUEST_NUM)
                    })
                    .GroupBy(c => c.SP_ID)
                    .Select(c => new
                    {
                        c.SP_ID,
                        SUM_REQUEST_NUM = c.SUM_REQUEST_NUM == null ? 0 : c.SUM_REQUEST_NUM
                    });

                var limitList = _dbContext.Query<DRUG_LIMIT>()
                .LeftJoin(query, (a, b) => a.SP_ID == b.SP_ID)
                .Select((a, b) => new
                {
                    a.SP_ID,
                    LEFTOVER = position == "1" ? a.INSIDE_OCTOBER - (b.SUM_REQUEST_NUM == null ? 0 : b.SUM_REQUEST_NUM) :
                    a.OUTSIDE_OCTOBER - (b.SUM_REQUEST_NUM == null ? 0 : b.SUM_REQUEST_NUM)
                });

                var requestDet = await _dbContext.Query<DRUG_REQUEST_DET>()
                .Where(a => a.REQUEST_ID == requestId)
                .LeftJoin(limitList, (a, b) => a.SP_ID == b.SP_ID)
                .Select((a, b) => new
                {
                    a.REQUEST_DET_ID,
                    a.REQUEST_ID,
                    a.SP_ID,
                    a.SP_NAME,
                    a.SP_CODE,
                    a.SP_SIZE,
                    a.TYPE_ID,
                    a.TYPE_NAME,
                    a.TYPE_CODE,
                    a.PRODUCE,
                    a.UNIT,
                    a.REQUEST_NUM,
                    a.MEMO,
                    a.PURPOSE,
                    a.CREATE_USERID,
                    a.CREATEDATE,
                    a.MODIFY_USERID,
                    a.MODIFYDATE,
                    b.LEFTOVER
                })
                .GetGridData(null);
                return requestDet;
            }
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
                    c.SP_NAME,
                    c.SP_CODE,
                    c.SP_SIZE,
                    c.TYPE_ID,
                    c.TYPE_NAME,
                    c.TYPE_CODE,
                    c.PRODUCE,
                    c.UNIT,
                    c.REQUEST_NUM,
                    c.MEMO,
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
