using Gksyb.Common.Office;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;

namespace EAM.Material.Services
{
    public class BaseSpHouseService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;
        private readonly ICorpService _corpService;

        public BaseSpHouseService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession, ICorpService corpService)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userSession = userSession;
            _corpService = corpService;
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
                    { "BCCode@#HouseType", "house_type" },
                    { "SpHouse", (Expression<Func<SP_HOUSE, bool>>)(x => true) },
                });
                data.TryAdd("Corp", await _corpService.ComboxDataAsync());
                return AjaxResult.Success(data);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }

        /// <summary>
        /// 获取仓库下拉数据
        /// </summary>
        public async Task<List<ComboxData>> HouseComboxDataAsync()
        {
            var data = await _comboxDataService.Get(new Dictionary<string, object>()
            {
                { "SpHouseName", (Expression<Func<SP_HOUSE, bool>>)(x => true) },
            });
            return data["SpHouseName"];
        }

        /// <summary>
        /// 根据ID获取数据
        /// </summary>
        public async Task<SP_HOUSE> GetAsync(string houseId)
        {
            return await _dbContext.Query<SP_HOUSE>().FirstOrDefaultAsync(c => c.HOUSE_ID == houseId);
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var qry = _dbContext.Query<SP_HOUSE>(x => _dbContext.Query<CF_CORP>(cc => cc.CORPID == x.DEPT_ID && cc.CORP_PATH.StartsWith(_userSession.ParentCompany.TreeNode)).Any());
            return await qry.GetGridData(request);
        }

        /// <summary>
        /// 保存
        /// </summary>
        public async Task<AjaxResult> SaveAsync(SaveRequest<SP_HOUSE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new { c.HOUSE_NAME, c.HOUSE_CODE, c.HOUSE_TYPE, c.PRE_HOUSE_ID, c.DEPT_ID, c.DEPT_NAME, c.MEMO },
                c => a => a.HOUSE_ID == c.HOUSE_ID,
                BeforeAdd, BeforeUpdate, BeforeDelete, orgin: true);
        }

        /// <summary>
        /// 新增前
        /// </summary>
        private async Task BeforeAdd(SP_HOUSE entity)
        {
            if (entity.HOUSE_ID.IsNullOrWhiteSpace())
            {
                entity.HOUSE_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            if (entity.DEPT_ID.IsNullOrWhiteSpace())
            {
                entity.DEPT_ID = _userSession.ParentCompany.CorpID;
                entity.DEPT_NAME = _userSession.ParentCompany.SName;
            }
            await Handle(entity);
        }

        /// <summary>
        /// 更新前
        /// </summary>
        private async Task BeforeUpdate(SP_HOUSE entity)
        {
            await Handle(entity);
        }

        /// <summary>
        /// 删除前
        /// </summary>
        private async Task BeforeDelete(SP_HOUSE entity)
        {
            var sonHouses = await _dbContext.Query<SP_HOUSE>(x => x.PRE_HOUSE_ID == entity.HOUSE_ID).ToListAsync();
            sonHouses.ForEach(x =>
            {
                x.PRE_HOUSE_ID = entity.PRE_HOUSE_ID;
                x.HOUSE_CODE = null;
            });
            await _dbContext.UpdateAsync<SP_HOUSE>(x => x.PRE_HOUSE_ID == entity.HOUSE_ID, x => new SP_HOUSE
            {
                PRE_HOUSE_ID = entity.PRE_HOUSE_ID,
                HOUSE_CODE = null
            });
            foreach (var son in sonHouses)
            {
                await UpdateCodeCascade(son);
                await _dbContext.UpdateAsync<SP_HOUSE>(x => x.HOUSE_ID == son.HOUSE_ID, x => new SP_HOUSE
                {
                    HOUSE_CODE = son.HOUSE_CODE
                });
            }
        }

        /// <summary>
        /// 检查和预处理
        /// </summary>
        private async Task Handle(SP_HOUSE entity)
        {
            await UpdateCodeCascade(entity);
        }

        /// <summary>
        /// 逐级更新编码
        /// </summary>
        private async Task UpdateCodeCascade(SP_HOUSE entity)
        {
            if (!entity.PRE_HOUSE_ID.IsNullOrWhiteSpace())
            {
                var preHouse = await _dbContext.QueryByKeyAsync<SP_HOUSE>(entity.PRE_HOUSE_ID);
                if (!entity.HOUSE_CODE.IsNullOrWhiteSpace() && entity.HOUSE_CODE.Length >= 3 && entity.HOUSE_CODE[..^3] == preHouse.HOUSE_CODE) return;
                var cur_code = await _dbContext.Query<SP_HOUSE>(x => x.HOUSE_CODE.StartsWith(preHouse.HOUSE_CODE) && x.HOUSE_CODE.Length == preHouse.HOUSE_CODE.Length + 3)
                    .MaxAsync(x => x.HOUSE_CODE);
                entity.HOUSE_CODE = cur_code.IsNullOrWhiteSpace()
                   ? $"{preHouse.HOUSE_CODE}001"
                   : $"{cur_code[..^3]}{long.Parse(cur_code[^3..]) + 1:D3}";
            }
            else
            {
                if (entity.HOUSE_CODE.IsNullOrWhiteSpace() || entity.HOUSE_CODE.Length != 3)
                {
                    var cur_code = await _dbContext.Query<SP_HOUSE>(x => string.IsNullOrWhiteSpace(x.PRE_HOUSE_ID))
                        .MaxAsync(x => x.HOUSE_CODE);
                    entity.HOUSE_CODE = cur_code.IsNullOrWhiteSpace() ? "001" : $"{long.Parse(cur_code) + 1:D3}";
                }
                else
                {
                    var curHouse = await _dbContext.QueryByKeyAsync<SP_HOUSE>(entity.HOUSE_ID);
                    if (curHouse != null && entity.HOUSE_CODE == curHouse.HOUSE_CODE) return;
                }
            }

            var keySet = new HashSet<string>() { entity.HOUSE_ID };
            var list = new Queue<SP_HOUSE>();
            list.Enqueue(entity);
            while (list.Any())
            {
                var parent = list.Dequeue();
                await UpdateRelatedTable(parent);
                var childList = await _dbContext.Query<SP_HOUSE>(x => x.PRE_HOUSE_ID == parent.HOUSE_ID)
                    .OrderBy(x => x.HOUSE_ID)
                    .ToListAsync();
                for (var i = 0; i < childList.Count; ++i)
                {
                    var child = childList[i];
                    MessageException.ThrowIf(keySet.Contains(child.HOUSE_ID), "出现上级链路循环");
                    keySet.Add(child.HOUSE_ID);
                    _dbContext.TrackEntity(child);
                    child.HOUSE_CODE = $"{parent.HOUSE_CODE}{i + 1:D3}";
                    await _dbContext.UpdateAsync(child);
                    list.Enqueue(child);
                }
            }
        }

        /// <summary>
        /// 更新关联表的仓库数据
        /// </summary>
        private async Task UpdateRelatedTable(SP_HOUSE entity)
        {
            await _dbContext.UpdateAsync<SP_STORE>(x => x.HOUSE_ID == entity.HOUSE_ID, x => new SP_STORE
            {
                HOUSE_NAME = entity.HOUSE_NAME,
                HOUSE_CODE = entity.HOUSE_CODE
            });
            await _dbContext.UpdateAsync<SP_INSTORE_DET>(x => x.HOUSE_ID == entity.HOUSE_ID, x => new SP_INSTORE_DET
            {
                HOUSE_NAME = entity.HOUSE_NAME,
                HOUSE_CODE = entity.HOUSE_CODE
            });
        }

        /// <summary>
        /// 获取树形结构
        /// </summary>
        public async Task<AjaxResult> TreeAsync()
        {
            var qry = _dbContext.Query<SP_HOUSE>(x => _dbContext.Query<CF_CORP>(cc => cc.CORPID == x.DEPT_ID && cc.CORP_PATH.StartsWith(_userSession.ParentCompany.TreeNode)).Any());
            var list = await qry.Select(c => new
            {
                c.HOUSE_ID,
                c.HOUSE_NAME,
                c.HOUSE_CODE,
                c.PRE_HOUSE_ID,
                c.DEPT_ID,
                c.DEPT_NAME
            }).OrderBy(c => c.HOUSE_CODE)
            .ToListAsync();
            return AjaxResult.Success(list, "成功");
        }

        /// <summary>
        /// Excel导入
        /// </summary>
        public async Task<AjaxResult> ImportAsync(IFormFile formFile)
        {
            await _dbContext.UseTransactionAsync(async () =>
            {
                await formFile.Import<SP_HOUSE>(async c =>
                {
                    if (!c.PRE_HOUSE_ID.IsNullOrWhiteSpace())
                    {
                        var sp_house = await _dbContext.Query<SP_HOUSE>(x => x.HOUSE_NAME == c.PRE_HOUSE_ID).FirstOrDefaultAsync();
                        MessageException.ThrowIf(sp_house == null, $"上级仓库「{c.PRE_HOUSE_ID}」不存在，请核对");
                        c.PRE_HOUSE_ID = sp_house?.HOUSE_ID;
                    }
                    else c.PRE_HOUSE_ID = null;
                    var request = new SaveRequest<SP_HOUSE> { Added = new List<SP_HOUSE> { c } };
                    await SaveAsync(request);
                });
            });
            return AjaxResult.Success("导入成功");
        }
    }
}
