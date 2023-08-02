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
using Microsoft.AspNetCore.Mvc;

namespace EAM.Special.Services
{
    public class DrugCollectRequestService : IDrugCollectRequestService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly IUserService _userService;

        public DrugCollectRequestService(IDbContext dbContext, IComboxDataService comboxDataService, IUserService userService)
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
            var list = await _dbContext.Query<DRUG_COLLECT_REQUEST>().Select(c => new
            {
                c.COLLECT_REQUEST_ID,
                c.COLLECT_ID,
                c.COLLECT_DET_ID,
                c.REQUEST_DET_ID,
                c.SP_ID,
                c.REQUEST_CODE,
                c.COLLECT_NUM,
                c.REQUEST_USER,
                c.REQUEST_USERID,
                c.SP_CODE,
                c.SP_NAME,
                c.SP_TYPE,
                c.SP_DAIMA,
                c.SP_TUHAO,
                c.SP_ENGNAME,
                c.BRAND,
                c.OTHER_CODE,
                c.UNIT,
                c.FACTORY,
                c.REQUEST_NUM,
                c.CHECK_NUM,
                c.COLLECT_MONEY,
                c.DEPT_NAME,
                c.DEPT_ID,
                c.SEC_DEPT,
                c.SEC_DEPTID,
                c.MEMO,
                c.TYPE_CODE,
                c.TYPE_NAME,
                c.TYPE_ID,
                c.TAX_PRICE,
                c.TAX_MONEY,
                c.NOTAX_PRICE,
                c.NOTAX_MONEY,
                c.IS_FULLBUY,
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
        public async Task<DRUG_COLLECT_REQUEST> GetAsync(string id)
        {
            var query = await _dbContext.Query<DRUG_COLLECT_REQUEST>().Where(c => c.COLLECT_REQUEST_ID == id).FirstAsync();
            return query;
        }

        /// <summary>
        /// 根据COLLECT_ID获取列表
        /// </summary>
        /// <param name="collectId"></param>
        /// <returns></returns>
        public async Task<GridData> GetCertainCollectIdAsync(string collectId)
        {
            var list = await _dbContext.Query<DRUG_COLLECT_REQUEST>()
                .Where(c => c.COLLECT_ID == collectId)
                .Select(c => new
                {
                    c.COLLECT_REQUEST_ID,
                    c.COLLECT_ID,
                    c.COLLECT_DET_ID,
                    c.REQUEST_DET_ID,
                    c.SP_ID,
                    c.REQUEST_CODE,
                    c.COLLECT_NUM,
                    c.REQUEST_USER,
                    c.REQUEST_USERID,
                    c.SP_CODE,
                    c.SP_NAME,
                    c.SP_TYPE,
                    c.SP_DAIMA,
                    c.SP_TUHAO,
                    c.SP_ENGNAME,
                    c.BRAND,
                    c.OTHER_CODE,
                    c.UNIT,
                    c.FACTORY,
                    c.REQUEST_NUM,
                    c.CHECK_NUM,
                    c.COLLECT_MONEY,
                    c.DEPT_NAME,
                    c.DEPT_ID,
                    c.SEC_DEPT,
                    c.SEC_DEPTID,
                    c.MEMO,
                    c.TYPE_CODE,
                    c.TYPE_NAME,
                    c.TYPE_ID,
                    c.TAX_PRICE,
                    c.TAX_MONEY,
                    c.NOTAX_PRICE,
                    c.NOTAX_MONEY,
                    c.IS_FULLBUY,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                }).GetGridData(null);
            return list;
        }

        /// <summary>
        /// 获取需要药品SP_ID的需求
        /// </summary>
        /// <param name="spId"></param>
        /// <returns></returns>
        public async Task<List<DRUG_COLLECT_REQUEST>> GetCertainSpIdAsync(string spId)
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
                .Where((a, b) => b.SP_ID == spId)
                .LeftJoin(query, (a, b, c) => b.REQUEST_DET_ID == c.REQUEST_DET_ID)
                .Where((a, b, c) => c.IS_FULLBUY == "0" || c.IS_FULLBUY == null)
                .Select((a, b, c) => new DRUG_COLLECT_REQUEST
                {
                    REQUEST_DET_ID = b.REQUEST_DET_ID,
                    SP_ID = b.SP_ID,
                    SP_NAME = b.SP_NAME,
                    SP_CODE = b.SP_CODE,
                    SP_TYPE = b.SP_TYPE,
                    SP_DAIMA = b.SP_DAIMA,
                    SP_TUHAO = b.SP_TUHAO,
                    SP_ENGNAME = b.SP_ENGNAME,
                    BRAND = b.BRAND,
                    OTHER_CODE = b.OTHER_CODE,
                    FACTORY = b.FACTORY,
                    UNIT = b.UNIT,
                    //申请数量为剩余未采购的申请数量
                    REQUEST_NUM = b.REQUEST_NUM - (c.COLLECT_NUM == null ? 0 : c.COLLECT_NUM),
                    REQUEST_CODE = a.REQUEST_CODE,
                    REQUEST_USER = a.REQUEST_USER,
                    REQUEST_USERID = a.CREATE_USERID,
                    DEPT_NAME = a.DEPT_NAME,
                    DEPT_ID = a.DEPT_ID,
                    SEC_DEPT = a.SEC_DEPT,
                    SEC_DEPTID = a.SEC_DEPTID,
                })
                .ToListAsync();
            return list;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAsync(SaveRequest<DRUG_COLLECT_REQUEST> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.COLLECT_REQUEST_ID,
                    c.COLLECT_ID,
                    c.COLLECT_DET_ID,
                    c.REQUEST_DET_ID,
                    c.SP_ID,
                    c.REQUEST_CODE,
                    c.COLLECT_NUM,
                    c.REQUEST_USER,
                    c.REQUEST_USERID,
                    c.SP_CODE,
                    c.SP_NAME,
                    c.SP_TYPE,
                    c.SP_DAIMA,
                    c.SP_TUHAO,
                    c.SP_ENGNAME,
                    c.BRAND,
                    c.OTHER_CODE,
                    c.UNIT,
                    c.FACTORY,
                    c.REQUEST_NUM,
                    c.CHECK_NUM,
                    c.COLLECT_MONEY,
                    c.DEPT_NAME,
                    c.DEPT_ID,
                    c.SEC_DEPT,
                    c.SEC_DEPTID,
                    c.MEMO,
                    c.TYPE_CODE,
                    c.TYPE_NAME,
                    c.TYPE_ID,
                    c.TAX_PRICE,
                    c.TAX_MONEY,
                    c.NOTAX_PRICE,
                    c.NOTAX_MONEY,
                    c.IS_FULLBUY,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                },
                c => a => a.COLLECT_REQUEST_ID == c.COLLECT_REQUEST_ID
                , BeforeAdd, null, null, false, null, null);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(DRUG_COLLECT_REQUEST entity)
        {
            if (entity.COLLECT_REQUEST_ID.IsNullOrEmpty())
            {
                entity.COLLECT_REQUEST_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(DRUG_COLLECT_REQUEST entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(DRUG_COLLECT_REQUEST entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 保存后验证
        /// </summary>
        private async Task AfterSave(List<DRUG_COLLECT_REQUEST> added, List<DRUG_COLLECT_REQUEST> updated, List<DRUG_COLLECT_REQUEST> deleted)
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
