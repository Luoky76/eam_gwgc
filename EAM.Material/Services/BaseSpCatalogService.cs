using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using System.Linq.Expressions;

namespace EAM.Material.Services
{
    public class BaseSpCatalogService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxService;
        private readonly UserSession _userSession;

        public BaseSpCatalogService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxService = comboxDataService;
            _userSession = userSession;
        }


        public async Task<AjaxResult> ComboxData()
        {
            var data = await _comboxService.Get(new Dictionary<string, object>(){
                { "BasePurtype", (Expression<Func <BC_CODE, bool>>)null},
                { "SpTypeName", null},
                { "SpCatalogName", null},
                { "SpUnit", null},
            });

            return AjaxResult.Success(data);
        }
        /// <summary>
        /// 获取树形结构
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> TreeAsync()
        {
            var spcatalogData = await _dbContext.Query<BASE_SPCATALOG>().Where(c => c.IS_CANCEL != "1" && c.IS_RECOVERY != "1").ToListAsync();
            var spcatalogList = spcatalogData.Select(c => new
            {
                c.SP_CODE,
                c.SP_NAME,
                c.SP_ID,
                c.TYPE_NAME,
                c.TYPE_ID,
                PARENTID = (string.IsNullOrWhiteSpace(c.TYPE_ID)) ? c.TYPE_ID : c.TYPE_ID,
                TYPE = "1",
                ICON = "fa fa-group"
            }).OrderBy(c => c.SP_CODE).ToList();

            var typeData = await _dbContext.Query<BASE_SPTYPE>().Where(c => c.IS_CANCEL != "1").ToListAsync();
            var typeList = typeData.Select(c => new
            {
                SP_CODE = c.TYPE_CODE,
                SP_NAME = c.TYPE_NAME,
                SP_ID = c.TYPE_ID,
                c.TYPE_NAME,
                c.TYPE_ID,
                PARENTID = (string.IsNullOrWhiteSpace(c.PRE_TYPEID) || c.PRE_TYPEID == "0") ? "ROOT" : c.PRE_TYPEID,
                TYPE = "0",
                ICON = "fa fa-cog"
            }).OrderBy(c => c.SP_CODE).ToList();

            /*
             * 原有左侧物资目录树中会包含物资节点，现只含物资类别节点
             */
            //spcatalogList = spcatalogList.Concat(typeList).ToList();
            spcatalogList = typeList;

            spcatalogList.Add(new
            {
                SP_CODE = "ROOT",
                SP_NAME = "物资目录",
                SP_ID = "ROOT",
                TYPE_NAME = "",
                TYPE_ID = "",
                PARENTID = "",
                TYPE = "-1",
                ICON = "fa fa-sitemap"
            });
            return AjaxResult.Success(spcatalogList, "成功");
        }


        /// <summary>
        /// 根据ID获取记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<BASE_SPCATALOG> GetAsync(object id)
        {
            string sid = id.ToString();
            var query = await _dbContext.Query<BASE_SPCATALOG>().Where(c => c.SP_ID == sid).FirstAsync();
            return query;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<BASE_SPCATALOG>().Select(c => new
            {
                c.TYPE_ID,
                c.TYPE_NAME,
                c.SP_ID,
                c.SP_NAME,
                c.SP_CODE,
                c.SP_SIZE,
                c.PURTYPE_NAME,
                c.MEMO,
                c.PURTYPE_ID,
                c.UNIT,
                c.PRODUCE,
                c.WARRANTY,
                c.TYPE_CODE,
                c.IS_RECOVERY,
                c.IS_CANCEL,
            }).GetGridData(request);
            return list;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAsync(SaveRequest<BASE_SPCATALOG> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.TYPE_ID,
                    c.TYPE_NAME,
                    c.SP_ID,
                    c.SP_NAME,
                    c.SP_CODE,
                    c.SP_SIZE,
                    c.PURTYPE_NAME,
                    c.MEMO,
                    c.UNIT,
                    c.PURTYPE_ID,
                    c.PRODUCE,
                    c.WARRANTY,
                    c.TYPE_CODE,
                    c.IS_RECOVERY,
                    c.IS_CANCEL,
                },
                c => a => a.SP_ID == c.SP_ID
                , BeforeAdd);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(BASE_SPCATALOG entity)
        {
            var model = await _dbContext.Query<BASE_SPCATALOG>(x => x.TYPE_ID == entity.TYPE_ID).Select(x => Sql.Max(x.SP_CODE)).FirstOrDefaultAsync();
            var index = string.IsNullOrEmpty(model) ? 1 : model.Substring(model.Length - 4).CastTo<int>() + 1;
            entity.SP_CODE = $"{entity.TYPE_CODE}-{index.ToString("D4")}";
            entity.SP_ID = GuidHelper.NewSnowflakeId().ToString();
            await Task.CompletedTask;
        }
    }
}
