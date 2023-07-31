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
                c.REQUEST_SPTYPE,
                c.SRC_CODE,
                c.POSITION,
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
                    c.REQUEST_SPTYPE,
                    c.SRC_CODE,
                    c.POSITION,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                },
                c => a => a.REQUEST_ID == c.REQUEST_ID
                , BeforeAdd, null, null, false, null, null);
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
                //示例DR2023072100001
                var sysdate = await _dbContext.GetSysdate();
                string dateCode = sysdate.Value.ToString("yyyyMMdd");
                string headCode = "DR";
                string newCode = headCode + dateCode + "00001";

                //查看编码是否已存在
                var list1 = await _dbContext.Query<DRUG_REQUEST>()
                    .Select(a => a.REQUEST_CODE)
                    .Where(a => a == newCode)
                    .ToListAsync();

                if (list1.Any())
                {
                    var list2 = await _dbContext.Query<DRUG_REQUEST>()
                    .Select(a => new
                    {
                        MAX_REQUEST_CODE = Sql.Max(a.REQUEST_CODE)
                    })
                    .ToListAsync();
                    if (list2.Any())
                    {
                        string lastCode = list2[0].MAX_REQUEST_CODE;
                        lastCode = lastCode.Substring(headCode.Length);
                        long cnt = long.Parse(lastCode);
                        ++cnt;
                        lastCode = cnt.ToString();
                        newCode = headCode + lastCode;
                    }
                }
                entity.REQUEST_CODE = newCode;
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
    }
}
