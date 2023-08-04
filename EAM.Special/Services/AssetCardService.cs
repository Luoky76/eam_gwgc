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
    public class AssetCardService : IAssetCardService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly IUserService _userService;
        private readonly ICorpService _corpService;

        public AssetCardService(IDbContext dbContext, IComboxDataService comboxDataService, IUserService userService, ICorpService corpService)
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
            var list = await _dbContext.Query<ASSET_CARD>()
                .GetGridData(request);
            return list;
        }

        /// <summary>
        /// 无形资产列表（软件）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> SoftwareListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<ASSET_CARD>()
                .Where(c => c.IS_TANGIBLE == "0")
                .Select(c => new
                {
                    c.ASSET_ID,
                    c.AUDITING,
                    c.IS_TANGIBLE,
                    c.ASSET_CODE,
                    c.PUR_DATE,
                    c.ASSET_NAME,
                    c.SHORT_NAME,
                    c.VERSION,
                    c.SERIAL_NUM,
                    c.DEPT_ID,
                    c.DEPT_NAME,
                    c.DEPT_CODE,
                    c.LICENSE_NUM,
                    c.PRODUCT_TYPE,
                    c.MANAGER_USERID,
                    c.MANAGER_USER,
                    c.MODIFY_USERID,
                    c.MEMO
                })
                .GetGridData(request);
            return list;
        }

        /// <summary>
        /// 固定资产列表（设备）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> DeviceListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<ASSET_CARD>()
                .Where(c => c.IS_TANGIBLE == "1")
                .Select(c => new
                {
                    c.ASSET_ID,
                    c.AUDITING,
                    c.IS_TANGIBLE,
                    c.ASSET_CODE,
                    c.ASSET_NAME,
                    c.TYPE_ID,
                    c.TYPE_NAME,
                    c.TYPE_CODE,
                    c.ASSETNO,
                    c.BRAND,
                    c.ASSET_TYPE,
                    c.DEPT_ID,
                    c.DEPT_NAME,
                    c.DEPT_CODE,
                    c.PERSON,
                    c.MDEPT_ID,
                    c.MDEPT_NAME,
                    c.CARD_USER,
                    c.INSTALL_SITE,
                    c.USE_STATE,
                    c.SERIAL_NUM,
                    c.PUR_DATE,
                    c.PROVIDER_NAME,
                    c.MANAGER_USERID,
                    c.MANAGER_USER,
                    c.MODIFY_USERID,
                    c.CONFIG_INFO,
                    c.MEMO
                })
                .GetGridData(request);
            return list;
        }

        /// <summary>
        /// 根据ID获取单行记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ASSET_CARD> GetAsync(string id)
        {
            var query = await _dbContext.Query<ASSET_CARD>()
                .Where(c => c.ASSET_ID == id)
                .FirstAsync();
            return query;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAsync(SaveRequest<ASSET_CARD> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.ASSET_ID,
                    c.AUDITING,
                    c.IS_TANGIBLE,
                    c.ASSET_CODE,
                    c.ASSET_NAME,
                    c.SHORT_NAME,
                    c.LICENSE_NUM,
                    c.INSTALL_SITE,
                    c.TYPE_NAME,
                    c.ASSETNO,
                    c.BRAND,
                    c.ASSET_TYPE,
                    c.SERIAL_NUM,
                    c.CONFIG_INFO,
                    c.USE_STATE,
                    c.DOMAIN,
                    c.COMPUTER_NAME,
                    c.IP_ADDRESS,
                    c.MAC_ADDRESS,
                    c.OUTER_NET,
                    c.PUR_DATE,
                    c.VERSION,
                    c.TYPE_CODE,
                    c.CARD_USER,
                    c.PERSON,
                    c.MDEPT_ID,
                    c.MDEPT_NAME,
                    c.DEPT_ID,
                    c.DEPT_NAME,
                    c.DEPT_CODE,
                    c.SEC_DEPTID,
                    c.SEC_DEPT,
                    c.TYPE_ID,
                    c.MANAGER_USERID,
                    c.MANAGER_USER,
                    c.SYSTEM_TYPE,
                    c.SYSTEM_COPYRIGHT,
                    c.SYSTEM_BAK,
                    c.REMOTE_TOOL,
                    c.PROVIDER_NAME,
                    c.PRODUCT_TYPE,
                    c.MEMO,
                    c.CREATE_USERID,
                    c.CREATEDATE,
                    c.MODIFY_USERID,
                    c.MODIFYDATE
                },
                c => a => a.ASSET_ID == c.ASSET_ID
                , BeforeAdd, null, null, false, null, null);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(ASSET_CARD entity)
        {
            if (entity.ASSET_ID.IsNullOrEmpty())
            {
                entity.ASSET_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            //自动添加记录编码
            if (entity.ASSET_CODE.IsNullOrEmpty())
            {
                //示例AC2023072100001
                var sysdate = await _dbContext.GetSysdate();
                string dateCode = sysdate.Value.ToString("yyyyMMdd");
                string headCode = "AC";
                string newCode = headCode + dateCode + "00001";

                //查看编码是否已存在
                var list1 = await _dbContext.Query<ASSET_CARD>()
                    .Select(a => a.ASSET_CODE)
                    .Where(a => a == newCode)
                    .ToListAsync();

                if (list1.Any())
                {
                    var list2 = await _dbContext.Query<ASSET_CARD>()
                    .Select(a => new
                    {
                        MAX_ASSET_CODE = Sql.Max(a.ASSET_CODE)
                    })
                    .ToListAsync();
                    if (list2.Any())
                    {
                        string lastCode = list2[0].MAX_ASSET_CODE;
                        lastCode = lastCode.Substring(headCode.Length);
                        long cnt = long.Parse(lastCode);
                        ++cnt;
                        lastCode = cnt.ToString();
                        newCode = headCode + lastCode;
                    }
                }
                entity.ASSET_CODE = newCode;
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(ASSET_CARD entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(ASSET_CARD entity)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 保存后验证
        /// </summary>
        private async Task AfterSave(List<ASSET_CARD> added, List<ASSET_CARD> updated, List<ASSET_CARD> deleted)
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
                    { "AssetStatus", null },
                    { "AssetProductType", null }
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
