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
    public class DrugLimitService : IDrugLimitService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;

        public DrugLimitService(IDbContext dbContext, IComboxDataService comboxDataService)
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
            var list = await _dbContext.Query<DRUG_LIMIT>().Select(c => new
            {
                c.LIMIT_ID,
                c.SP_ID,
                c.SP_CODE,
                c.SP_NAME,
                c.SP_TYPE,
                c.UNIT,
                c.INSIDE_APRIL,
                c.OUTSIDE_APRIL,
                c.INSIDE_OCTOBER,
                c.OUTSIDE_OCTOBER,
                c.CREATE_USERID,
                c.CREATEDATE,
                c.MODIFY_USERID,
                c.MODIFYDATE
            }).GetGridData(request);
            return list;
        }

        /// <summary>
        /// 获取除特定需求单外，剩余药品数量列表
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public async Task<GridData> ExtendListAsync(string requestId)
        {
            var list = await _dbContext.Query<DRUG_REQUEST>()
                .Where(a => a.REQUEST_ID == requestId)
                .Select(a => new
                {
                    a.REQUEST_YEAR,
                    a.REQUEST_MONTH,
                    a.DEPT_ID,
                    a.POSITION
                })
                .ToListAsync();

            int year = 0, month = 0;
            string deptID = "";
            string position = "";
            if (list.Any())
            {
                year = list[0].REQUEST_YEAR.Value;
                month = list[0].REQUEST_MONTH.Value;
                deptID = list[0].DEPT_ID;
                //"1"港内，"2"港外
                position = list[0].POSITION;
            }
            else return null;

            if (month >= 4 && month <= 9)
            {
                var query = _dbContext.Query<DRUG_REQUEST>()
                    //已提交 需求类型为月度或厂修（不计入临时） 排除本需求单 相同部门 相同位置（港内或港外） 相同年份 4~9月
                    .Where(a => a.AUDITING == "1" && (a.REQUEST_TYPE == "1" || a.REQUEST_TYPE == "2")
                    && a.REQUEST_ID != requestId && a.DEPT_ID == deptID && a.POSITION == position
                    && a.REQUEST_YEAR == year && a.REQUEST_MONTH >= 4 && a.REQUEST_MONTH <= 9)
                    .LeftJoin<DRUG_REQUEST_DET>((a, b) => a.REQUEST_ID == b.REQUEST_ID)
                    .Select((a, b) => new {
                        b.SP_ID,
                        SUM_REQUEST_NUM = Sql.Sum(b.REQUEST_NUM)
                    })
                    .GroupBy(c => c.SP_ID)
                    .Select(c => new
                    {
                        c.SP_ID,
                        SUM_REQUEST_NUM = c.SUM_REQUEST_NUM == null ? 0 : c.SUM_REQUEST_NUM
                    });

                var limitList = await _dbContext.Query<DRUG_LIMIT>()
                .LeftJoin(query,(a,b)=>a.SP_ID==b.SP_ID)
                .Select((a, b) => new
                {
                    a.LIMIT_ID,
                    a.SP_ID,
                    a.SP_CODE,
                    a.SP_NAME,
                    a.SP_TYPE,
                    a.UNIT,
                    LEFTOVER = position == "1" ? a.INSIDE_APRIL - (b.SUM_REQUEST_NUM == null ? 0 : b.SUM_REQUEST_NUM):
                    a.OUTSIDE_APRIL - (b.SUM_REQUEST_NUM == null ? 0 : b.SUM_REQUEST_NUM)
                }).GetGridData(null);

                return limitList;
            }
            else if (month <= 3)
            {
                var query = _dbContext.Query<DRUG_REQUEST>()
                    //已提交 需求类型为月度或厂修（不计入临时） 排除本需求单 相同部门 相同位置（港内或港外） 同年1~3月或去年10~12月
                    .Where(a => a.AUDITING == "1" && (a.REQUEST_TYPE == "1" || a.REQUEST_TYPE == "2")
                    && a.REQUEST_ID != requestId && a.DEPT_ID == deptID && a.POSITION == position
                    && (a.REQUEST_YEAR == year && a.REQUEST_MONTH <= 3 || a.REQUEST_YEAR == year - 1 && a.REQUEST_MONTH >= 10))
                    .LeftJoin<DRUG_REQUEST_DET>((a, b) => a.REQUEST_ID == b.REQUEST_ID)
                    .Select((a, b) => new {
                        b.SP_ID,
                        SUM_REQUEST_NUM = Sql.Sum(b.REQUEST_NUM)
                    })
                    .GroupBy(c => c.SP_ID)
                    .Select(c => new
                    {
                        c.SP_ID,
                        SUM_REQUEST_NUM = c.SUM_REQUEST_NUM == null ? 0 : c.SUM_REQUEST_NUM
                    });

                var limitList = await _dbContext.Query<DRUG_LIMIT>()
                .LeftJoin(query, (a, b) => a.SP_ID == b.SP_ID)
                .Select((a, b) => new
                {
                    a.LIMIT_ID,
                    a.SP_ID,
                    a.SP_CODE,
                    a.SP_NAME,
                    a.SP_TYPE,
                    a.UNIT,
                    LEFTOVER = position == "1" ? a.INSIDE_OCTOBER - (b.SUM_REQUEST_NUM == null ? 0 : b.SUM_REQUEST_NUM) :
                    a.OUTSIDE_OCTOBER - (b.SUM_REQUEST_NUM == null ? 0 : b.SUM_REQUEST_NUM)
                }).GetGridData(null);

                return limitList;
            }
            else
            {
                var query = _dbContext.Query<DRUG_REQUEST>()
                    //已提交 需求类型为月度或厂修（不计入临时） 排除本需求单 相同部门 相同位置（港内或港外） 同年10~12月或下一年1~3月
                    .Where(a => a.AUDITING == "1" && (a.REQUEST_TYPE == "1" || a.REQUEST_TYPE == "2")
                    && a.REQUEST_ID != requestId && a.DEPT_ID == deptID && a.POSITION == position
                    && (a.REQUEST_YEAR == year && a.REQUEST_MONTH >= 10 || a.REQUEST_YEAR == year + 1 && a.REQUEST_MONTH <= 3))
                    .LeftJoin<DRUG_REQUEST_DET>((a, b) => a.REQUEST_ID == b.REQUEST_ID)
                    .Select((a, b) => new {
                        b.SP_ID,
                        SUM_REQUEST_NUM = Sql.Sum(b.REQUEST_NUM)
                    })
                    .GroupBy(c => c.SP_ID)
                    .Select(c => new
                    {
                        c.SP_ID,
                        SUM_REQUEST_NUM = c.SUM_REQUEST_NUM == null ? 0 : c.SUM_REQUEST_NUM
                    });

                var limitList = await _dbContext.Query<DRUG_LIMIT>()
                .LeftJoin(query, (a, b) => a.SP_ID == b.SP_ID)
                .Select((a, b) => new
                {
                    a.LIMIT_ID,
                    a.SP_ID,
                    a.SP_CODE,
                    a.SP_NAME,
                    a.SP_TYPE,
                    a.UNIT,
                    LEFTOVER = position == "1" ? a.INSIDE_OCTOBER - (b.SUM_REQUEST_NUM == null ? 0 : b.SUM_REQUEST_NUM) :
                    a.OUTSIDE_OCTOBER - (b.SUM_REQUEST_NUM == null ? 0 : b.SUM_REQUEST_NUM)
                }).GetGridData(null);

                return limitList;
            }
        }

        /// <summary>
        /// 根据ID获取单行记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<DRUG_LIMIT> GetAsync(string id)
        {
            var query = await _dbContext.Query<DRUG_LIMIT>().Where(c => c.LIMIT_ID == id).FirstAsync();
            return query;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAsync(SaveRequest<DRUG_LIMIT> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.LIMIT_ID,
                    c.SP_ID,
                    c.SP_CODE,
                    c.SP_NAME,
                    c.SP_TYPE,
                    c.UNIT,
                    c.INSIDE_APRIL,
                    c.OUTSIDE_APRIL,
                    c.INSIDE_OCTOBER,
                    c.OUTSIDE_OCTOBER,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                },
                c => a => a.LIMIT_ID == c.LIMIT_ID
                , BeforeAdd, null, null, false, null, null);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(DRUG_LIMIT entity)
        {
            if (entity.LIMIT_ID.IsNullOrEmpty())
            {
                entity.LIMIT_ID = GuidHelper.NewSnowflakeId().ToString();
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(DRUG_LIMIT entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(DRUG_LIMIT entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 保存后验证
        /// </summary>
        private async Task AfterSave(List<DRUG_LIMIT> added, List<DRUG_LIMIT> updated, List<DRUG_LIMIT> deleted)
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
