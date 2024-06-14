using Chloe;
using EAM.Special.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using System.Linq.Expressions;

namespace EAM.Special.Services
{
    public class DrugRequestService : IDrugRequestService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly IUserService _userService;

        public DrugRequestService(IDbContext dbContext, IComboxDataService comboxDataService, IUserService userService)
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
            var list = await _dbContext.Query<DRUG_REQUEST>().Select(c => new
            {
                c.REQUEST_ID,
                c.AUDITING,
                c.REQUEST_CODE,
                c.REQUEST_MONTH,
                c.REQUEST_YEAR,
                c.DEPT_ID,
                c.DEPT_NAME,
                c.DEPT_CODE,
                c.SHIP_ID,
                c.SHIP_NAME,
                c.SHIP_CODE,
                c.SEC_DEPTID,
                c.SEC_DEPT,
                c.MEMO,
                c.REQUEST_TYPE,
                c.FORM_ID,
                c.SRC_CODE,
                c.POSITION,
                c.REQUEST_USER,
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
        public async Task<DRUG_REQUEST> GetAsync(string id)
        {
            var query = await _dbContext.Query<DRUG_REQUEST>().Where(c => c.REQUEST_ID == id).FirstAsync();
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
        public async Task<AjaxResult> SaveAsync(SaveRequest<DRUG_REQUEST> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.REQUEST_ID,
                    c.AUDITING,
                    c.REQUEST_CODE,
                    c.REQUEST_MONTH,
                    c.REQUEST_YEAR,
                    c.DEPT_ID,
                    c.DEPT_NAME,
                    c.DEPT_CODE,
                    c.SHIP_ID,
                    c.SHIP_NAME,
                    c.SHIP_CODE,
                    c.SEC_DEPTID,
                    c.SEC_DEPT,
                    c.MEMO,
                    c.REQUEST_TYPE,
                    c.FORM_ID,
                    c.SRC_CODE,
                    c.POSITION,
                    c.REQUEST_USER,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                },
                c => a => a.REQUEST_ID == c.REQUEST_ID
                , BeforeAdd, BeforeUpdate, null, false, null, AfterSave);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(DRUG_REQUEST entity)
        {
            if (entity.REQUEST_ID.IsNullOrEmpty())
            {
                entity.REQUEST_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            //自动添加需求计划单号
            if (entity.REQUEST_CODE.IsNullOrEmpty())
            {
                //示例DR2023070001
                string headCode = "DR";
                var sysdate = await _dbContext.GetSysdate();
                string dateCode = sysdate.Value.ToString("yyyyMM");
                string newCode = headCode + dateCode + "0000";
                string model = await _dbContext.Query<DRUG_REQUEST>(a => a.REQUEST_CODE.Contains(headCode + dateCode))
                    .Select(a => Sql.Max(a.REQUEST_CODE) ?? newCode).FirstOrDefaultAsync();
                entity.REQUEST_CODE = headCode + (long.Parse(model.Substring(headCode.Length)) + 1).ToString();
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(DRUG_REQUEST entity)
        {
            //对于即将提交的非临时需求进行验证（临时需求不受药品数量限制）
            if (entity.AUDITING == "1" && entity.REQUEST_TYPE != "3")
            {
                string requestId = entity.REQUEST_ID;
                int year = entity.REQUEST_YEAR.Value;
                int month = entity.REQUEST_MONTH.Value;
                string deptID = entity.DEPT_ID;
                //"1"港内，"2"港外
                string position = entity.POSITION;

                Expression<Func<DRUG_REQUEST, bool>> whereCondition;
                if (month >= 4 && month <= 9)
                {
                    //已提交或为即将提交的该需求 需求类型为月度或厂修（不计入临时） 排除本需求单 相同部门 相同位置（港内或港外） 相同年份 4~9月
                    whereCondition = a => (a.AUDITING == "1" || a.REQUEST_ID == requestId)
                        && (a.REQUEST_TYPE == "1" || a.REQUEST_TYPE == "2")
                        && a.DEPT_ID == deptID && a.POSITION == position
                        && a.REQUEST_YEAR == year && a.REQUEST_MONTH >= 4 && a.REQUEST_MONTH <= 9;
                }
                else if (month <= 3)
                {
                    //已提交或为即将提交的该需求 需求类型为月度或厂修（不计入临时） 排除本需求单 相同部门 相同位置（港内或港外） 同年1~3月或去年10~12月
                    whereCondition = a => (a.AUDITING == "1" || a.REQUEST_ID == requestId)
                        && (a.REQUEST_TYPE == "1" || a.REQUEST_TYPE == "2")
                        && a.DEPT_ID == deptID && a.POSITION == position
                        && (a.REQUEST_YEAR == year && a.REQUEST_MONTH <= 3 || a.REQUEST_YEAR == year - 1 && a.REQUEST_MONTH >= 10);
                }
                else
                {
                    //已提交或为即将提交的该需求 需求类型为月度或厂修（不计入临时） 排除本需求单 相同部门 相同位置（港内或港外） 同年10~12月或下一年1~3月
                    whereCondition = a => (a.AUDITING == "1" || a.REQUEST_ID == requestId)
                        && (a.REQUEST_TYPE == "1" || a.REQUEST_TYPE == "2")
                        && a.DEPT_ID == deptID && a.POSITION == position
                        && (a.REQUEST_YEAR == year && a.REQUEST_MONTH >= 10 || a.REQUEST_YEAR == year + 1 && a.REQUEST_MONTH <= 3);
                }

                var query = _dbContext.Query<DRUG_REQUEST>()
                    //已提交或为即将提交的该需求 需求类型为月度或厂修（不计入临时） 相同部门 相同位置（港内或港外） 相同年份 4~9月
                    .Where(whereCondition)
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

                if (month >= 4 && month <= 9)
                {
                    var limitList = await _dbContext.Query<DRUG_LIMIT>()
                    .LeftJoin(query, (a, b) => a.SP_ID == b.SP_ID)
                    .Select((a, b) => new
                    {
                        a.SP_ID,
                        a.SP_NAME,
                        LEFTOVER = position == "1" ? a.INSIDE_APRIL - (b.SUM_REQUEST_NUM == null ? 0 : b.SUM_REQUEST_NUM) :
                        a.OUTSIDE_APRIL - (b.SUM_REQUEST_NUM == null ? 0 : b.SUM_REQUEST_NUM)
                    })
                    .Where(c => c.LEFTOVER < 0)
                    .ToListAsync();

                    //存在剩余限量<0的药品，取消提交
                    if (limitList.Any())
                    {
                        throw new MessageException("药品" + limitList[0].SP_NAME + "超过数量限制，请重新导入该药品");
                    }
                }
                else
                {
                    var limitList = await _dbContext.Query<DRUG_LIMIT>()
                    .LeftJoin(query, (a, b) => a.SP_ID == b.SP_ID)
                    .Select((a, b) => new
                    {
                        a.SP_ID,
                        a.SP_NAME,
                        LEFTOVER = position == "1" ? a.INSIDE_OCTOBER - (b.SUM_REQUEST_NUM == null ? 0 : b.SUM_REQUEST_NUM) :
                        a.OUTSIDE_OCTOBER - (b.SUM_REQUEST_NUM == null ? 0 : b.SUM_REQUEST_NUM)
                    })
                    .Where(c => c.LEFTOVER < 0)
                    .ToListAsync();

                    //存在剩余限量<0的药品，取消提交
                    if (limitList.Any())
                    {
                        throw new MessageException("药品" + limitList[0].SP_NAME + "超过数量限制，请重新导入该药品");
                    }
                }
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(DRUG_REQUEST entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 保存后验证
        /// </summary>
        private async Task AfterSave(List<DRUG_REQUEST> added, List<DRUG_REQUEST> updated, List<DRUG_REQUEST> deleted)
        {
            //级联删除药品需求明细DRUG_REQUEST_DET
            foreach (var entity in deleted)
            {
                await _dbContext.DeleteAsync<DRUG_REQUEST_DET>(c => c.REQUEST_ID == entity.REQUEST_ID);
            }
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
                    { "RequestType", null },
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

        /// <summary>
        /// 撤销提交
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        public async Task<AjaxResult> RevokeAsync(string requestId)
        {
            var list = await _dbContext.Query<DRUG_REQUEST_DET>(a => a.REQUEST_ID == requestId)
                .InnerJoin<DRUG_COLLECT_REQUEST>((a, b) => a.REQUEST_DET_ID == b.REQUEST_DET_ID)
                .InnerJoin<DRUG_COLLECT>((a, b, c) => b.COLLECT_ID == c.COLLECT_ID)
                .Select((a, b, c) => new
                {
                    c.COLLECT_CODE
                })
                .Distinct()
                .ToListAsync();

            if (list.Any())
            {
                string message = "";
                for (int i = 0; i < list.Count; ++i)
                {
                    message += i == 0 ? list[i].COLLECT_CODE : "、" + list[i].COLLECT_CODE;
                }
                return AjaxResult.Error("撤销失败！\n以下采购订单已包含该需求：" + message);
            }
            _dbContext.Update<DRUG_REQUEST>(c => c.REQUEST_ID == requestId, c => new DRUG_REQUEST
            {
                AUDITING = "0"
            });
            return AjaxResult.Success("撤销成功");
        }
    }
}
