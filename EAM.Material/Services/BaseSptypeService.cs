using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Services
{
    public class BaseSptypeService : BaseService, IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxService;
        private readonly UserSession _userSession;

        public BaseSptypeService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxService = comboxDataService;
            _userSession = userSession;
        }


        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public async Task<AjaxResult> ComboxDataAsync()
        {
            var data = await _comboxService.Get(new Dictionary<string, object>(){
                { "BasePurtype", null },
                { "SpTypeName", null },
            });

            return AjaxResult.Success(data);
        }

        /// <summary>
        /// 获取树形结构
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> TreeAsync()
        {
            var list = await _dbContext.Query<BASE_SPTYPE>().Where(c => c.IS_CANCEL != "1").ToListAsync();
            var data = list.Select(c => new
            {
                c.TYPE_CODE,
                c.TYPE_NAME,
                c.TYPE_ID,
                PARENTID = (string.IsNullOrWhiteSpace(c.PRE_TYPEID) || c.PRE_TYPEID == "0") ? "ROOT" : c.PRE_TYPEID,
                ICON = "fa fa-group"
            }).OrderBy(c => c.TYPE_CODE).ToList();
            data.Add(new { TYPE_CODE = "ROOT", TYPE_NAME = "物资分类", TYPE_ID = "ROOT", PARENTID = "", ICON = "fa fa-sitemap" });
            return AjaxResult.Success(data, "成功");
        }

        /// <summary>
        /// 根据ID获取记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<BASE_SPTYPE> GetAsync(object id)
        {
            string sid = id.ToString();
            var query = await _dbContext.Query<BASE_SPTYPE>().Where(c => c.TYPE_ID == sid).FirstAsync();
            return query;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<BASE_SPTYPE>().Select(c => new
            {
                c.TYPE_ID,
                c.TYPE_CODE,
                c.TYPE_NAME,
                c.IS_CANCEL,
                c.TYPE_LEVEL,
                c.PRE_TYPEID,
                c.PURTYPE_NAME,
                c.PURTYPE_ID,
                c.NOVALID_DATE,
                c.MEMO
            }).GetGridData(request);
            return list;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAsync(SaveRequest<BASE_SPTYPE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.TYPE_ID,
                    c.TYPE_CODE,
                    c.TYPE_NAME,
                    c.IS_CANCEL,
                    c.TYPE_LEVEL,
                    c.PRE_TYPEID,
                    c.PURTYPE_NAME,
                    c.PURTYPE_ID,
                    c.NOVALID_DATE,
                    c.MEMO
                },
                c => a => a.TYPE_ID == c.TYPE_ID
                , BeforeAdd, BeforeUpdate, BeforeDelete, false, null, AfterSave);
        }

        /// <summary>
        /// 添加前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(BASE_SPTYPE entity)
        {
            entity.TYPE_ID = GuidHelper.NewSnowflakeId().ToString();
            var query = await _dbContext.Query<BASE_SPTYPE>()
                .Where(c => c.TYPE_ID == entity.PRE_TYPEID && c.PRE_TYPEID == entity.TYPE_ID || c.TYPE_ID == entity.PRE_TYPEID && c.TYPE_ID == entity.TYPE_ID)
                .FirstOrDefaultAsync();
            if (query != null)
            {
                throw new MessageException("上级节点只能为父节点！");
            }
            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新前验证
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(BASE_SPTYPE entity)
        {
            var query = await _dbContext.Query<BASE_SPTYPE>()
                .Where(c => c.TYPE_ID == entity.PRE_TYPEID && c.PRE_TYPEID == entity.TYPE_ID || c.TYPE_ID == entity.PRE_TYPEID && c.TYPE_ID == entity.TYPE_ID)
                .FirstOrDefaultAsync();
            if (query != null)
            {
                throw new MessageException("上级节点只能为父节点！");
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除前验证
        /// </summary>
        private async Task BeforeDelete(BASE_SPTYPE entity)
        {
            // 删除分类时，同时删除所有子类
            // 所有被删除的分类，将该类下的物资改为被删除分类的上级分类，或临时类别
            var newPreType = new BASE_SPTYPE();
            if (entity.PRE_TYPEID.IsNullOrWhiteSpace())
            {
                newPreType = await _dbContext.Query<BASE_SPTYPE>(x => x.TYPE_NAME == "临时类别").FirstOrDefaultAsync();
            }
            else
            {
                newPreType = await _dbContext.Query<BASE_SPTYPE>(x => x.TYPE_ID == entity.PRE_TYPEID).FirstOrDefaultAsync();
            }
            if (newPreType == null)
            {
                // 无可替代的分类，判断是否已有物资在分类下，有则不允许删除
                var anySp = await _dbContext.Query<BASE_SPCATALOG>(x => x.TYPE_CODE.StartsWith(entity.TYPE_CODE)).AnyAsync();
                if (anySp)
                {
                    MessageException.Throw("该分类下已有物资，无法删除！");
                }
            }
            await _dbContext.DeleteAsync<BASE_SPTYPE>(x => x.TYPE_CODE.StartsWith(entity.TYPE_CODE));
            await _dbContext.UpdateAsync<BASE_SPCATALOG>(x => x.TYPE_CODE.StartsWith(entity.TYPE_CODE), x => new BASE_SPCATALOG
            {
                TYPE_ID = newPreType.TYPE_ID,
                TYPE_NAME = entity.TYPE_NAME,
                TYPE_CODE = entity.TYPE_CODE
            });
            await _dbContext.UpdateAsync<SP_APPLY_DETAIL>(x => x.TYPE_CODE.StartsWith(entity.TYPE_CODE), x => new SP_APPLY_DETAIL
            {
                TYPE_ID = newPreType.TYPE_ID,
                TYPE_NAME = entity.TYPE_NAME,
                TYPE_CODE = entity.TYPE_CODE
            });
            await _dbContext.UpdateAsync<SP_COLLECT_DET>(x => x.TYPE_CODE.StartsWith(entity.TYPE_CODE), x => new SP_COLLECT_DET
            {
                TYPE_ID = newPreType.TYPE_ID,
                TYPE_NAME = entity.TYPE_NAME,
                TYPE_CODE = entity.TYPE_CODE
            });
            await _dbContext.UpdateAsync<SP_COLLECT_REQUEST>(x => x.TYPE_CODE.StartsWith(entity.TYPE_CODE), x => new SP_COLLECT_REQUEST
            {
                TYPE_ID = newPreType.TYPE_ID,
                TYPE_NAME = entity.TYPE_NAME,
                TYPE_CODE = entity.TYPE_CODE
            });
            await _dbContext.UpdateAsync<SP_OUTAPP_DET>(x => x.TYPE_CODE.StartsWith(entity.TYPE_CODE), x => new SP_OUTAPP_DET
            {
                TYPE_ID = newPreType.TYPE_ID,
                TYPE_NAME = entity.TYPE_NAME,
                TYPE_CODE = entity.TYPE_CODE
            });
            await _dbContext.UpdateAsync<SP_OUTBACK_DET>(x => x.TYPE_CODE.StartsWith(entity.TYPE_CODE), x => new SP_OUTBACK_DET
            {
                TYPE_ID = newPreType.TYPE_ID,
                TYPE_NAME = entity.TYPE_NAME,
                TYPE_CODE = entity.TYPE_CODE
            });
            await _dbContext.UpdateAsync<SP_OUTSTORE_DET>(x => x.TYPE_CODE.StartsWith(entity.TYPE_CODE), x => new SP_OUTSTORE_DET
            {
                TYPE_ID = newPreType.TYPE_ID,
                TYPE_NAME = entity.TYPE_NAME,
                TYPE_CODE = entity.TYPE_CODE
            });
        }

        /// <summary>
        /// 保存后处理
        /// </summary>
        private async Task AfterSave(List<BASE_SPTYPE> added, List<BASE_SPTYPE> updated, List<BASE_SPTYPE> deleted)
        {
            await Task.CompletedTask;
        }
    }
}
