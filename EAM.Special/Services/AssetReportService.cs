using Chloe;
using DocumentFormat.OpenXml.Wordprocessing;
using EAM.Special.Interfaces;
using Gksyb.Common;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Microsoft.IdentityModel.Tokens;

namespace EAM.Special.Services
{
    public class AssetReportService : IAssetReportService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly IUserService _userService;
        private readonly ICorpService _corpService;

        public AssetReportService(IDbContext dbContext, IComboxDataService comboxDataService, IUserService userService, ICorpService corpService)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userService = userService;
            _corpService = corpService;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<ASSET_REPORT>()
                .GetGridData(request);
            return list;
        }

        /// <summary>
        /// 维修申请列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ApplyListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<ASSET_REPORT>()
                .LeftJoin<ASSET_CARD>((a, b) => a.ASSET_ID == b.ASSET_ID)
                .Select((a, b) => new
                {
                    a.REPORT_ID,
                    a.AUDITING_APPLY,
                    a.APPLY_CODE,
                    a.APPLY_DATE,
                    a.ASSET_ID,
                    a.APPLY_USER_ID,
                    a.APPLY_USER_NAME,
                    a.APPLY_TEL,
                    a.APPLY_DEPTID,
                    a.APPLY_DEPT,
                    a.FAILURE_TIME,
                    a.FAILURE_DESCRIBE,
                    a.MEMO,
                    
                    b.ASSET_NAME,
                    b.ASSET_CODE,
                    b.ASSETNO,
                    b.TYPE_ID,
                    b.TYPE_NAME,
                    b.DEPT_ID,
                    b.DEPT_NAME,
                    b.CARD_USER,
                    b.PERSON,
                    b.ASSET_TYPE,
                    b.BRAND
                })
                .GetGridData(request);
            return list;
        }

        /// <summary>
        /// 维修实施列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> CheckListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<ASSET_REPORT>()
                .Where(c => c.AUDITING_APPLY == "1")
                .Select(c => new
                {
                    
                })
                .GetGridData(request);
            return list;
        }

        /// <summary>
        /// 委外维修列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> OutsourceListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<ASSET_REPORT>()
                .Where(c => c.AUDITING_CHECK == "1" && c.REPORT_STATE == "4")
                .Select(c => new
                {

                })
                .GetGridData(request);
            return list;
        }

        /// <summary>
        /// 维修验收列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> AcceptListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<ASSET_REPORT>()
                //维修实施已提交的非外修状态 或 委外维修已提交的外修状态
                .Where(c => c.AUDITING_CHECK == "1" && c.REPORT_STATE != "4" || c.AUDITING_OUTSOURCE == "1" && c.REPORT_STATE == "4")
                .Select(c => new
                {

                })
                .GetGridData(request);
            return list;
        }

        /// <summary>
        /// 根据ID获取单行记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ASSET_REPORT> GetAsync(string id)
        {
            var query = await _dbContext.Query<ASSET_REPORT>()
                .Where(c => c.REPORT_ID == id)
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
        public async Task<AjaxResult> SaveAsync(SaveRequest<ASSET_REPORT> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    
                },
                c => a => a.REPORT_ID == c.REPORT_ID
                , BeforeAdd, null, null, false, null, null);
        }

        /// <summary>
        /// 创建编码
        /// </summary>
        /// <param name="headCode"></param>
        /// <returns></returns>
        private async Task<string> CreateCode(string headCode)
        {
            //示例HEADCODE2023072100001
            var sysdate = await _dbContext.GetSysdate();
            string dateCode = sysdate.Value.ToString("yyyyMMdd");
            string newCode = headCode + dateCode + "00001";

            //查看编码是否已存在
            var list1 = await _dbContext.Query<ASSET_REPORT>()
                .Select(a => a.APPLY_CODE)
                .Where(a => a == newCode)
                .ToListAsync();

            if (list1.Any())
            {
                var list2 = await _dbContext.Query<ASSET_REPORT>()
                .Select(a => new
                {
                    MAX_APPLY_CODE = Sql.Max(a.APPLY_CODE)
                })
                .ToListAsync();
                if (list2.Any())
                {
                    string lastCode = list2[0].MAX_APPLY_CODE;
                    lastCode = lastCode.Substring(headCode.Length);
                    long cnt = long.Parse(lastCode);
                    ++cnt;
                    lastCode = cnt.ToString();
                    newCode = headCode + lastCode;
                }
            }
            return newCode;
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(ASSET_REPORT entity)
        {
            if (entity.REPORT_ID.IsNullOrEmpty())
            {
                entity.REPORT_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            //自动添加记录编码
            if (entity.APPLY_CODE.IsNullOrEmpty())
            {
                entity.APPLY_CODE = await CreateCode("AM");
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(ASSET_REPORT entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(ASSET_REPORT entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 保存后验证
        /// </summary>
        private async Task AfterSave(List<ASSET_REPORT> added, List<ASSET_REPORT> updated, List<ASSET_REPORT> deleted)
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
                    { "User", null },
                });
                //data.TryAdd("User", await _userService.ComboxDataAsync());
                data.TryAdd("Corp", await _corpService.ComboxDataAsync());

                return AjaxResult.Success(data);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }
    }
}
