using Chloe;
using Gksyb.Common;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;
using System.Linq.Expressions;

namespace EAM.Device.Services
{
    public class DeviceTypeService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxService;

        public DeviceTypeService(IDbContext dbContext, IComboxDataService comboxService)
        {
            _dbContext = dbContext;
            _comboxService = comboxService;
        }

        public async Task<AjaxResult> ComboxData()
        {
            var data = await _comboxService.Get(new Dictionary<string, object>(){
                { "DeviceTypeCode",null},
                { "DeviceTypeName", (Expression<Func<BASE_DEVICETYPE, bool>>)null},
            });

            return AjaxResult.Success(data);
        }

        /// <summary>
        /// 获取树形结构
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> TreeAsync()
        {
            var list = await _dbContext.Query<BASE_DEVICETYPE>().Where(c => c.STATUS == "1").ToListAsync();
            var data = list.Select(c => new
            {
                c.TYPE_CODE,
                c.TYPE_NAME,
                c.TYPE_ID,
                PARENTID = (string.IsNullOrWhiteSpace(c.PRE_TYPEID) || c.PRE_TYPEID == "0") ? "ROOT" : c.PRE_TYPEID,
                ICON = "fa fa-group"
            }).OrderBy(c => c.TYPE_CODE).ToList();
            data.Add(new { TYPE_CODE = "ROOT", TYPE_NAME = "设备分类", TYPE_ID = "ROOT", PARENTID = "", ICON = "fa fa-sitemap" });
            return AjaxResult.Success(data, "成功");
        }

        /// <summary>
        /// 根据ID获取记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<BASE_DEVICETYPE> GetAsync(object id)
        {
            string sid = id.ToString();
            var query = await _dbContext.Query<BASE_DEVICETYPE>().Where(c => c.TYPE_ID == sid).FirstAsync();
            return query;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<BASE_DEVICETYPE>().Select(c => new
            {
                c.TYPE_ID,
                c.TYPE_CODE,
                c.TYPE_NAME,
                c.STATUS,
                c.IS_SPEC,
                c.TYPE_DESC,
                c.WORK_YEAR,
                c.TYPE_LEVEL,
                c.PRE_TYPEID,
                PRE_TYPENAME = _dbContext.Query<BASE_DEVICETYPE>().Where(r => r.TYPE_ID == c.PRE_TYPEID).First().TYPE_NAME,
                c.MEMO
            }).GetGridData(request);
            return list;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AjaxResult> SaveAsync(SaveRequest<BASE_DEVICETYPE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.TYPE_ID,
                    c.TYPE_CODE,
                    c.TYPE_NAME,
                    c.STATUS,
                    c.IS_SPEC,
                    c.TYPE_DESC,
                    c.WORK_YEAR,
                    c.TYPE_LEVEL,
                    c.PRE_TYPEID,
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
        private async Task BeforeAdd(BASE_DEVICETYPE entity)
        {
            //默认值
            var def = "000";
            if (entity.PRE_TYPEID != "")
            {
                //取出父级节点的最大值
                var pacode = await _dbContext.Query<BASE_DEVICETYPE>(x => x.TYPE_ID == entity.PRE_TYPEID)
                    .Select(x => x.TYPE_CODE)
                    .FirstOrDefaultAsync();
                var newchild = pacode + def;

                var childcode = await _dbContext.Query<BASE_DEVICETYPE>(x => x.PRE_TYPEID == entity.PRE_TYPEID)
                    .Select(x => Sql.Max(x.TYPE_CODE) ?? newchild)
                    .FirstOrDefaultAsync();
                int parentCodeValue = childcode.CastTo<int>() + 1;
                int numberOfDigits = childcode.ToString().Length;

                entity.TYPE_CODE = $"{parentCodeValue}".PadLeft(numberOfDigits, '0');
            }
            else
            {
                var parentcode = await _dbContext.Query<BASE_DEVICETYPE>()
                    .Select(x => Sql.Max(x.TYPE_CODE) ?? def)
                    .FirstOrDefaultAsync();
                entity.TYPE_CODE = (int.Parse(parentcode) + 1).ToString("D3");

            }

            entity.TYPE_ID = GuidHelper.NewSnowflakeId().ToString();
            var query = await _dbContext.Query<BASE_DEVICETYPE>()
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
        private async Task BeforeUpdate(BASE_DEVICETYPE entity)
        {

            var query = await _dbContext.Query<BASE_DEVICETYPE>()
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
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(BASE_DEVICETYPE entity)
        {
            //验证是否存在下属节点
            var query = await _dbContext.Query<BASE_DEVICETYPE>().Where(c => c.STATUS == "1" && c.PRE_TYPEID == entity.TYPE_ID).ToListAsync();
            if (query.Count > 0) throw new MessageException("该节点存在有效下级节点，不能删除！");

            await Task.CompletedTask;
        }

        private async Task AfterSave(List<BASE_DEVICETYPE> added, List<BASE_DEVICETYPE> updated, List<BASE_DEVICETYPE> deleted)
        {
            await Task.CompletedTask;
        }
    }
}