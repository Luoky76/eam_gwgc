using Chloe;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Core.Interfaces.General;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;

namespace EAM.Special.Services
{
    /// <summary>
    /// 低值品入账
    /// </summary>
    public class LowspareInService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly UserSession _userSession;
        private readonly IComboxDataService _comboxDataService;
        private readonly ICodeCreatorService _codeCreatorService;
        private DateTime? _Sysdate;

        /// <summary>
        /// 低值品入账
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="comboxDataService"></param>
        /// <param name="codeCreatorService"></param>
        /// <param name="userSession"></param>

        public LowspareInService(IDbContext dbContext, IComboxDataService comboxDataService, ICodeCreatorService codeCreatorService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _codeCreatorService = codeCreatorService;
            _userSession = userSession;

        }

        /// <summary>
        /// 获取数据库时间
        /// </summary>
        private DateTime? Sysdate
        {
            get
            {
                if (!_Sysdate.HasValue)
                {
                    _Sysdate = _dbContext.GetSysdate().Result();
                }
                return _Sysdate;
            }
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
                    { "UserDept", null },
                    { "SpcatalogCard", null },
                    { "LowType", null }
                });

                return AjaxResult.Success(data);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }


        /// <summary>
        /// 列表
        /// </summary>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            //return await _dbContext.Query<SPEC_LOWSPARE_IN>().GetGridData(request);
            var list = await _dbContext.Query<SPEC_LOWSPARE_IN>()
            .Select(c => new
            {
                c.AUDITING,
                c.IN_CODE,
                c.IN_DATE,
                c.SP_CODE,
                c.SP_NAME,
                c.SP_SIZE,
                c.PRODUCE,
                c.UNIT,
                c.TYPE_NAME,
                c.LOW_TYPE,
                c.USER_NAME,
                c.DEPT_NAME,
                c.IN_NUM,
                c.LOCATION,
                c.MEMO,
                c.HANDLE_USER,
                c.HANDLE_USERID,
                c.USER_ID,
                c.DEPT_ID,
                c.IN_ID,
                c.SP_ID,
                c.ADD_DATE,
                c.ADD_USERID,
                c.MODIFY_DATE,
                c.MODIFY_USERID,
                attach_id = _dbContext.Query<SYS_ATTACH>().Where(a => a.data_id == c.IN_ID.ToString() && a.table_name == "SPEC_LOWSPARE_IN").Count(),
            }).GetGridData(request);

            return list;
        }

        /// <summary>
        /// 通过ID查询记录
        /// </summary>
        /// <returns></returns>
        public async Task<SPEC_LOWSPARE_IN> GetAsync(string inId)
        {
            return await _dbContext.Query<SPEC_LOWSPARE_IN>().Where(c => c.IN_ID == inId).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAsync(SaveRequest<SPEC_LOWSPARE_IN> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING,
                    c.IN_CODE,
                    c.IN_DATE,
                    c.SP_CODE,
                    c.SP_NAME,
                    c.SP_SIZE,
                    c.PRODUCE,
                    c.UNIT,
                    c.TYPE_NAME,
                    c.LOW_TYPE,
                    c.USER_NAME,
                    c.DEPT_NAME,
                    c.IN_NUM,
                    c.LOCATION,
                    c.MEMO,
                    c.HANDLE_USER,
                    c.HANDLE_USERID,
                    c.USER_ID,
                    c.DEPT_ID,
                    c.IN_ID,
                    c.SP_ID,
                    c.ADD_DATE,
                    c.ADD_USERID,
                    c.MODIFY_DATE,
                    c.MODIFY_USERID
                },
                c => a => a.IN_ID == c.IN_ID
                , BeforeAdd, BeforeUpdate, null, false, null, AfterSave); ;
        }

        /// <summary>
        /// 提交
        /// </summary>
        /// <param name="sids"></param>
        /// <returns></returns>
        public async Task<int> SubmitAsync(List<string> sids)
        {
            return await _dbContext.UpdateAsync<SPEC_LOWSPARE_IN>(
                c => sids.Contains(c.IN_ID),
                c => new SPEC_LOWSPARE_IN
                {
                    AUDITING = "1"
                });
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(SPEC_LOWSPARE_IN entity)
        {
            entity.IN_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.IN_DATE = Sysdate;
            entity.ADD_USERID = _userSession.UserID.ToString();
            entity.ADD_DATE = Sysdate;

            if (entity.IN_CODE.IsNullOrWhiteSpace())
            {
                entity.IN_CODE = await _codeCreatorService.CreateCodeAsync<SPEC_LOWSPARE_IN>("RZ", a => a.IN_CODE);
            }
            if (entity.AUDITING.IsNullOrWhiteSpace())
            {
                entity.AUDITING = "0";
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(SPEC_LOWSPARE_IN entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 保存后验证
        /// </summary>
        private async Task AfterSave(List<SPEC_LOWSPARE_IN> added, List<SPEC_LOWSPARE_IN> updated, List<SPEC_LOWSPARE_IN> deleted)
        {

            foreach (var add in added)
            {
                var tz = new SPEC_LOWSPARE()
                {
                    SP_CODE = add.SP_CODE,
                    SP_NAME = add.SP_NAME,
                    SP_SIZE = add.SP_SIZE,
                    PRODUCE = add.PRODUCE,
                    LOWSPARE_NUM = add.IN_NUM,
                    USER_NAME = add.USER_NAME,
                    TYPE_NAME = add.TYPE_NAME,
                    DEPT_NAME = add.DEPT_NAME,
                    LOW_TYPE = add.LOW_TYPE,
                    LOCATION = add.LOCATION,
                    UNIT = add.UNIT,
                    DEPT_ID = add.DEPT_ID,
                    USER_ID = add.USER_ID,
                    SP_ID = add.SP_ID,
                    LOWSPARE_ID = GuidHelper.NewSnowflakeId().ToString(),
                    ADD_USERID = add.ADD_USERID,
                    ADD_DATE = add.ADD_DATE,
                    MODIFY_DATE = add.MODIFY_DATE,
                    MODIFY_USERID = add.MODIFY_USERID
                };
            }
            await Task.CompletedTask;
        }
    }
}
