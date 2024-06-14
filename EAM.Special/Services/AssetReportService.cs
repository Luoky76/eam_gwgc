using Chloe;
using EAM.Special.Interfaces;
using Gksyb.Common;
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
                .LeftJoin<ASSET_CARD>((a, b) => a.ASSET_ID == b.ASSET_ID)
                .Select((a, b) => new
                {
                    a.REPORT_ID,
                    a.AUDITING_APPLY,
                    a.REPORT_STATE,
                    a.APPLY_CODE,
                    a.APPLY_DATE,
                    a.ASSET_ID,
                    a.APPLY_USERID,
                    a.APPLY_USER,
                    a.APPLY_TEL,
                    a.APPLY_DEPTID,
                    a.APPLY_DEPT,
                    a.FAILURE_TIME,
                    a.FAILURE_DESCRIBE,
                    a.APPLY_MEMO,

                    a.AUDITING_CHECK,
                    a.CHECK_USERID,
                    a.CHECK_USER,
                    a.CHECK_DEPTID,
                    a.CHECK_DEPT,
                    a.CHECK_DATE,
                    a.CHECK_BEGIN,
                    a.CHECK_END,
                    a.FAILURE_CAUSE,
                    a.CHECK_METH,
                    a.CHECK_MEMO,

                    a.AUDITING_OUTSOURCE,
                    a.OUTSOURCE_DATE,
                    a.IS_UNDER_WARRANTY,
                    a.PROVIDER,
                    a.PROVIDER_TEL,
                    a.OUTSOURCE_MEMO,

                    a.AUDITING_ACCEPT,
                    a.ACCEPT_DATE,
                    a.ACCEPT_DESC,
                    a.ACCEPT_APPRAISE,

                    b.ASSET_NAME,
                    b.ASSET_CODE,
                    b.ASSETNO,
                    b.TYPE_ID,
                    b.TYPE_NAME,
                    b.DEPT_ID,
                    b.DEPT_NAME,
                    b.CARD_USER,
                    b.PERSON,
                    b.ASSET_SIZE,
                    b.PRODUCE
                })
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
                    a.AUDITING_CHECK,
                    a.REPORT_STATE,
                    a.APPLY_CODE,
                    a.APPLY_DATE,
                    a.ASSET_ID,
                    a.APPLY_USERID,
                    a.APPLY_USER,
                    a.APPLY_TEL,
                    a.APPLY_DEPTID,
                    a.APPLY_DEPT,
                    a.FAILURE_TIME,
                    a.FAILURE_DESCRIBE,
                    a.APPLY_MEMO,

                    b.ASSET_NAME,
                    b.ASSET_CODE,
                    b.ASSETNO,
                    b.TYPE_ID,
                    b.TYPE_NAME,
                    b.DEPT_ID,
                    b.DEPT_NAME,
                    b.CARD_USER,
                    b.PERSON,
                    b.ASSET_SIZE,
                    b.PRODUCE
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
                .LeftJoin<ASSET_CARD>((a, b) => a.ASSET_ID == b.ASSET_ID)
                .Select((a, b) => new
                {
                    a.REPORT_ID,
                    a.AUDITING_CHECK,
                    a.AUDITING_OUTSOURCE,
                    a.AUDITING_ACCEPT,
                    a.REPORT_STATE,
                    a.APPLY_CODE,
                    a.APPLY_DATE,
                    a.ASSET_ID,
                    a.APPLY_USERID,
                    a.APPLY_USER,
                    a.APPLY_TEL,
                    a.APPLY_DEPTID,
                    a.APPLY_DEPT,
                    a.FAILURE_TIME,
                    a.FAILURE_DESCRIBE,
                    a.APPLY_MEMO,

                    b.ASSET_NAME,
                    b.ASSET_CODE,
                    b.TYPE_ID,
                    b.TYPE_NAME,
                    b.DEPT_ID,
                    b.DEPT_NAME,
                    b.PERSON,

                    a.CHECK_USERID,
                    a.CHECK_USER,
                    a.CHECK_DEPTID,
                    a.CHECK_DEPT,
                    a.CHECK_DATE,
                    a.CHECK_BEGIN,
                    a.CHECK_END,
                    a.FAILURE_CAUSE,
                    a.CHECK_METH,
                    a.CHECK_MEMO
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
                .LeftJoin<ASSET_CARD>((a, b) => a.ASSET_ID == b.ASSET_ID)
                .Select((a, b) => new
                {
                    a.REPORT_ID,
                    a.AUDITING_OUTSOURCE,
                    a.AUDITING_ACCEPT,
                    a.APPLY_CODE,
                    a.OUTSOURCE_DATE,
                    a.ASSET_ID,
                    a.APPLY_USERID,
                    a.APPLY_USER,
                    a.APPLY_TEL,
                    a.APPLY_DEPTID,
                    a.APPLY_DEPT,
                    a.FAILURE_DESCRIBE,

                    b.ASSET_NAME,
                    b.ASSET_CODE,
                    b.ASSETNO,
                    b.TYPE_ID,
                    b.TYPE_NAME,
                    b.DEPT_ID,
                    b.DEPT_NAME,
                    b.CARD_USER,
                    b.PERSON,
                    b.ASSET_SIZE,
                    b.PRODUCE,
                    b.SERIAL_NUM,
                    b.INSTALL_SITE,

                    a.IS_UNDER_WARRANTY,
                    a.PROVIDER,
                    a.PROVIDER_TEL,
                    a.OUTSOURCE_MEMO
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
                .LeftJoin<ASSET_CARD>((a, b) => a.ASSET_ID == b.ASSET_ID)
                .Select((a, b) => new
                {
                    a.REPORT_ID,
                    a.AUDITING_ACCEPT,
                    a.REPORT_STATE,
                    a.APPLY_CODE,
                    a.APPLY_DATE,
                    a.ASSET_ID,
                    a.APPLY_USERID,
                    a.APPLY_USER,
                    a.APPLY_TEL,
                    a.APPLY_DEPTID,
                    a.APPLY_DEPT,
                    a.FAILURE_TIME,
                    a.FAILURE_DESCRIBE,
                    a.APPLY_MEMO,

                    a.CHECK_USERID,
                    a.CHECK_USER,
                    a.CHECK_DEPTID,
                    a.CHECK_DEPT,
                    a.CHECK_DATE,
                    a.CHECK_BEGIN,
                    a.CHECK_END,
                    a.FAILURE_CAUSE,
                    a.CHECK_METH,
                    a.CHECK_MEMO,

                    a.OUTSOURCE_DATE,
                    a.IS_UNDER_WARRANTY,
                    a.PROVIDER,
                    a.PROVIDER_TEL,
                    a.OUTSOURCE_MEMO,

                    a.ACCEPT_DATE,
                    a.ACCEPT_DESC,
                    a.ACCEPT_APPRAISE,

                    b.ASSET_NAME,
                    b.ASSET_CODE,
                    b.ASSETNO,
                    b.TYPE_ID,
                    b.TYPE_NAME,
                    b.DEPT_ID,
                    b.DEPT_NAME,
                    b.CARD_USER,
                    b.PERSON,
                    b.ASSET_SIZE,
                    b.PRODUCE,
                })
                .GetGridData(request);
            return list;
        }

        /// <summary>
        /// 根据ID获取单行记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ASSET_REPORT_AND_CARD> GetAsync(string id)
        {
            var query = await _dbContext.Query<ASSET_REPORT>()
                .Where(c => c.REPORT_ID == id)
                .LeftJoin<ASSET_CARD>((a, b) => a.ASSET_ID == b.ASSET_ID)
                .Select((a, b) => new ASSET_REPORT_AND_CARD
                {
                    REPORT_ID = a.REPORT_ID,
                    AUDITING_APPLY = a.AUDITING_APPLY,
                    AUDITING_CHECK = a.AUDITING_CHECK,
                    AUDITING_ACCEPT = a.AUDITING_ACCEPT,
                    AUDITING_OUTSOURCE = a.AUDITING_OUTSOURCE,
                    REPORT_STATE = a.REPORT_STATE,
                    ASSET_ID = a.ASSET_ID,
                    APPLY_CODE = a.APPLY_CODE,
                    APPLY_DATE = a.APPLY_DATE,
                    APPLY_DEPTID = a.APPLY_DEPTID,
                    APPLY_DEPT = a.APPLY_DEPT,
                    APPLY_USERID = a.APPLY_USERID,
                    APPLY_USER = a.APPLY_USER,
                    APPLY_TEL = a.APPLY_TEL,
                    CHECK_DATE = a.CHECK_DATE,
                    ACCEPT_APPRAISE = a.ACCEPT_APPRAISE,
                    SEC_DEPTID = a.SEC_DEPTID,
                    SEC_DEPT = a.SEC_DEPT,
                    FAILURE_TIME = a.FAILURE_TIME,
                    FAILURE_DESCRIBE = a.FAILURE_DESCRIBE,
                    FAILURE_CAUSE = a.FAILURE_CAUSE,
                    APPLY_MEMO = a.APPLY_MEMO,
                    CHECK_METH = a.CHECK_METH,
                    CHECK_MEMO = a.CHECK_MEMO,
                    ACCEPT_DATE = a.ACCEPT_DATE,
                    ACCEPT_DESC = a.ACCEPT_DESC,
                    CHECK_DEPTID = a.CHECK_DEPTID,
                    CHECK_DEPT = a.CHECK_DEPT,
                    CHECK_USERID = a.CHECK_USERID,
                    CHECK_USER = a.CHECK_USER,
                    CHECK_BEGIN = a.CHECK_BEGIN,
                    CHECK_END = a.CHECK_END,
                    OUTSOURCE_DATE = a.OUTSOURCE_DATE,
                    IS_UNDER_WARRANTY = a.IS_UNDER_WARRANTY,
                    PROVIDER = a.PROVIDER,
                    PROVIDER_TEL = a.PROVIDER_TEL,
                    OUTSOURCE_MEMO = a.OUTSOURCE_MEMO,
                    CREATE_USERID = a.CREATE_USERID,
                    CREATEDATE = a.CREATEDATE,
                    MODIFY_USERID = a.MODIFY_USERID,
                    MODIFYDATE = a.MODIFYDATE,

                    ASSET_NAME = b.ASSET_NAME,
                    ASSET_CODE = b.ASSET_CODE,
                    ASSETNO = b.ASSETNO,
                    TYPE_ID = b.TYPE_ID,
                    TYPE_NAME = b.TYPE_NAME,
                    DEPT_ID = b.DEPT_ID,
                    DEPT_NAME = b.DEPT_NAME,
                    CARD_USER = b.CARD_USER,
                    PERSON = b.PERSON,
                    ASSET_SIZE = b.ASSET_SIZE,
                    PRODUCE = b.PRODUCE,
                    SERIAL_NUM = b.SERIAL_NUM,
                    INSTALL_SITE = b.INSTALL_SITE,
                })
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
                    c.REPORT_ID,
                    c.AUDITING_APPLY,
                    c.AUDITING_CHECK,
                    c.AUDITING_ACCEPT,
                    c.AUDITING_OUTSOURCE,
                    c.REPORT_STATE,
                    c.ASSET_ID,
                    c.APPLY_CODE,
                    c.APPLY_DATE,
                    c.APPLY_DEPTID,
                    c.APPLY_DEPT,
                    c.APPLY_USERID,
                    c.APPLY_USER,
                    c.APPLY_TEL,
                    c.CHECK_DATE,
                    c.ACCEPT_APPRAISE,
                    c.SEC_DEPTID,
                    c.SEC_DEPT,
                    c.FAILURE_TIME,
                    c.FAILURE_DESCRIBE,
                    c.FAILURE_CAUSE,
                    c.APPLY_MEMO,
                    c.CHECK_METH,
                    c.CHECK_MEMO,
                    c.ACCEPT_DATE,
                    c.ACCEPT_DESC,
                    c.CHECK_DEPTID,
                    c.CHECK_DEPT,
                    c.CHECK_USERID,
                    c.CHECK_USER,
                    c.CHECK_BEGIN,
                    c.CHECK_END,
                    c.OUTSOURCE_DATE,
                    c.IS_UNDER_WARRANTY,
                    c.PROVIDER,
                    c.PROVIDER_TEL,
                    c.OUTSOURCE_MEMO,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                },
                c => a => a.REPORT_ID == c.REPORT_ID
                , BeforeAdd, BeforeUpdate, null, false, null, null);
        }

        /// <summary>
        /// 创建编码
        /// </summary>
        /// <param name="headCode"></param>
        /// <returns></returns>
        private async Task<string> CreateCode(string headCode)
        {
            //示例HEADCODE2023070001
            var sysdate = await _dbContext.GetSysdate();
            string dateCode = sysdate.Value.ToString("yyyyMM");
            string newCode = headCode + dateCode + "0000";
            string model = await _dbContext.Query<ASSET_REPORT>(a => a.APPLY_CODE.Contains(headCode + dateCode))
                .Select(a => Sql.Max(a.APPLY_CODE) ?? newCode).FirstOrDefaultAsync();
            return headCode + (long.Parse(model.Substring(headCode.Length)) + 1).ToString();
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
            //设置记录状态初值为未提交
            if (entity.AUDITING_APPLY.IsNullOrEmpty())
            {
                entity.AUDITING_APPLY = "0";
            }
            if (entity.AUDITING_CHECK.IsNullOrEmpty())
            {
                entity.AUDITING_CHECK = "0";
            }
            if (entity.AUDITING_OUTSOURCE.IsNullOrEmpty())
            {
                entity.AUDITING_OUTSOURCE = "0";
            }
            if (entity.AUDITING_ACCEPT.IsNullOrEmpty())
            {
                entity.AUDITING_ACCEPT = "0";
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
                    { "AssetCard", null },
                    { "ReportState", null },
                    { "Appraise", null }
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
