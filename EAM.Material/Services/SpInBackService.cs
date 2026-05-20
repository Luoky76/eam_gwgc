using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Core.Interfaces.General;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Material.Services
{
    public class SpInBackService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly ICorpService _corpService;
        private readonly ICodeCreatorService _codeCreatorService;
        private readonly UserSession _userSession;
        private string masterID = string.Empty, errMsg = string.Empty;

        public SpInBackService(IDbContext dbContext, IComboxDataService comboxDataService, ICorpService corpService, ICodeCreatorService codeCreatorService, UserSession userSession)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _corpService = corpService;
            _codeCreatorService = codeCreatorService;
            _userSession = userSession;
        }

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public async Task<AjaxResult> ComboxDataAsync()
        {
            var data = await _comboxDataService.Get(new Dictionary<string, object>());
            data.TryAdd("Corp", await _corpService.ComboxDataAsync());
            return AjaxResult.Success(data);
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var query = await _dbContext.Query<SP_IN_BACK>().GetGridData(request);
            return query;
        }

        /// <summary>
        /// 获取记录
        /// </summary>
        public async Task<AjaxResult> GetAsync(string inBackId)
        {
            var query = await _dbContext.Query<SP_IN_BACK>().Where(x => x.IN_BACK_ID == inBackId).ToListAsync();

            return AjaxResult.Success(query);
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<AjaxResult> InListAsync()
        {
            var result = await _dbContext.JoinQuery<SP_INSTORE, SP_INSTORE_DET>((a, b) => new object[]
               {
                   JoinType.LeftJoin,a.IN_ID.Equals(b.IN_ID)
               })
               .Where((a, b) => a.AUDITING == "1")
               .Select((a, b) => new
               {
                   a.AUDITING,
                   a.IN_CODE,
                   a.IN_DATE,
                   a.ORDER_CODE,
                   a.PROVIDER_NAME,
                   a.INSTORE_MONEY,
                   a.PUR_USER,
                   a.USER_NAME,
                   a.CHK_USER,
                   a.DEPT_NAME,
                   a.MEMO,
                   a.IN_ID,
                   b.SP_CODE,
                   b.SP_NAME,
                   b.UNIT,
                   b.SP_SIZE,
                   b.PRODUCE,
                   b.STORE_ID,
                   b.COUNT,
                   b.DELIVERY_CODE,
                   b.STOCK_NAME,
                   b.PRICE,
                   b.MONEY,
                   b.APPLY_USER,
                   b.APPLY_NO,
                   b.INDET_ID
               })
               .ToListAsync();
            return AjaxResult.Success(result, "成功");
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> DetListAsync(GridRequest request)
        {
            var list = await _dbContext.Query<SP_INBACK_DET>().GetGridData(request);
            return list;
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> DetailListAsync(GridRequest request)
        {
            var result = await _dbContext.JoinQuery<SP_IN_BACK, SP_INBACK_DET>((a, b) => new object[]
               {
                   JoinType.LeftJoin,a.IN_BACK_ID.Equals(b.IN_BACK_ID)
               })
               .Where((a, b) => a.AUDITING == "1")
               .Select((a, b) => new
               {
                   a.AUDITING,
                   a.BACK_DATE,
                   a.BACK_CODE,
                   a.EDIT_USER,
                   a.SUM_MONEY,
                   a.MEMO,
                   a.IN_BACK_ID,
                   a.IN_CODE,
                   a.IN_ID,
                   a.ORDER_CODE,
                   a.DELIVERY_CODE,
                   b.SP_CODE,
                   b.SP_NAME,
                   b.UNIT,
                   b.SP_SIZE,
                   b.PRODUCE,
                   b.STORE_ID,
                   b.COUNT,
                   b.STOCK_NAME,
                   b.PRICE,
                   b.MONEY,
                   b.APPLY_USER,
                   b.APPLY_NO,
                   b.INDET_ID
               })
               .GetGridData(request);
            return result;
        }

        /// <summary>
        /// 同时保存主子表
        /// </summary>
        public async Task<AjaxResult> SaveAllAsync(SaveRequest<SP_IN_BACK> request, SaveRequest<SP_INBACK_DET> requestdet)
        {
            await _dbContext.UseTransactionAsync(async () =>
            {
                bool mainSuccess = false, detSuccess = false;
                var execResult = await _dbContext.SaveEntityAnsyc(request,
                     c => new
                     {
                         c.AUDITING,
                         c.IN_CODE,
                         c.IN_DATE,
                         c.DELIVERY_CODE,
                         c.BACK_CODE,
                         c.BACK_DATE,
                         c.ORDER_CODE,
                         c.PROVIDER_NAME,
                         c.PUR_USER,
                         c.CHK_USER,
                         c.EDIT_USER,
                         c.SUM_MONEY,
                         c.MEMO,
                         c.IN_ID,
                         c.IN_BACK_ID,
                     },
                     c => a => a.IN_BACK_ID == c.IN_BACK_ID
                     , BeforeAdd, BeforeUpdate, BeforeDelete, false, null, null);

                mainSuccess = !execResult.IsError;
                if (mainSuccess)  //主表是否保存成功
                {
                    requestdet = requestdet ?? new SaveRequest<SP_INBACK_DET>();

                    execResult = await _dbContext.SaveEntityAnsyc(requestdet,
                         c => new
                         {
                             c.SP_CODE,
                             c.SP_NAME,
                             c.UNIT,
                             c.SP_SIZE,
                             c.PRODUCE,
                             c.COUNT,
                             c.STOCK_NAME,
                             c.PRICE,
                             c.MONEY,
                             c.STORE_ID,
                             c.APPLY_USER,
                             c.APPLY_NO,
                             c.MEMO,
                             c.INDET_ID
                         },
                         c => a => a.INDET_ID == c.INDET_ID,
                         BeforeAddDet, BeforeUpdateDet, null, false, null, null);

                    detSuccess = !execResult.IsError;  //明细表是否保存成功
                }
                if (!mainSuccess || !detSuccess)
                {
                    if (string.IsNullOrWhiteSpace(errMsg)) errMsg = "保存失败";
                    throw new Exception(errMsg);
                }
            });
            return AjaxResult.Success("保存成功");
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        private async Task BeforeAdd(SP_IN_BACK entity)
        {
            entity.IN_BACK_ID = GuidHelper.NewSnowflakeId().ToString();
            masterID = entity.IN_BACK_ID;

            if (entity.EDIT_USERID.IsNullOrWhiteSpace())
            {
                entity.EDIT_USERID = _userSession.UserID.ToString();
                entity.EDIT_USER = _userSession.RealName;
            }

            if (entity.BACK_CODE.IsNullOrWhiteSpace())
            {
                entity.BACK_CODE = await _codeCreatorService.CreateCodeAsync<SP_IN_BACK>("RCH", a => a.BACK_CODE);
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(SP_IN_BACK request)
        {
            if (request.AUDITING == "1")
            {
                var det = await _dbContext.Query<SP_INSTORE_DET>(x => x.IN_ID == request.IN_ID).ToListAsync();

                foreach (var iten in det)
                {
                    await _dbContext.UpdateAsync<SP_STORE>(x => iten.STORE_ID.Contains(x.STORE_ID),
                    x => new SP_STORE
                    {
                        IS_BACK = "1",
                    });

                    await _dbContext.UpdateAsync<STORE_WATER>(x => iten.STORE_ID.Contains(x.STORE_ID),
                    x => new STORE_WATER
                    {
                        IS_BACK = "1",
                    });
                }
            }

            await Task.CompletedTask;
        }

        /// <summary>
        /// 提交
        /// </summary>
        public async Task<AjaxResult> SubmitAsync(List<string> sids)
        {
            if (sids == null || sids.Count == 0) return AjaxResult.Error("请选择行");

            await _dbContext.UseTransactionAsync(async () =>
            {
                foreach (var sid in sids)
                {
                    var entity = await _dbContext.Query<SP_IN_BACK>(x => x.IN_BACK_ID == sid).FirstOrDefaultAsync();
                    if (entity == null) continue;
                    if (entity.AUDITING == "1")
                    {
                        throw new Exception("该数据已提交，无法重复提交！");
                    }

                    entity.AUDITING = "1";
                    await BeforeUpdate(entity);
                    await _dbContext.UpdateAsync(entity);
                }
            });
            return AjaxResult.Success("提交成功");
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeDelete(SP_IN_BACK request)
        {
            await _dbContext.DeleteAsync<SP_INBACK_DET>(x => x.IN_BACK_ID == request.IN_BACK_ID);

            await Task.CompletedTask;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        private async Task BeforeAddDet(SP_INBACK_DET entity)
        {
            entity.INDET_ID = GuidHelper.NewSnowflakeId().ToString();
            entity.IN_BACK_ID = masterID;

            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeUpdateDet(SP_INBACK_DET request)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeDeleteDet(SP_INBACK_DET request)
        {
            await Task.CompletedTask;
        }
    }
}
