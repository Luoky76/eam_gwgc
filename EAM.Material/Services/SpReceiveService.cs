using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Core.Interfaces.General;
using Gksyb.Model;
using Gksyb.Model.Grid;
using System.Linq.Expressions;

namespace EAM.Material.Services
{
    public class SpReceiveService : IBaseService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;
        private readonly UserSession _userSession;
        private readonly ICodeCreatorService _codeCreatorService;

        public SpReceiveService(IDbContext dbContext, IComboxDataService comboxDataService, UserSession userSession, ICodeCreatorService codeCreatorService)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
            _userSession = userSession;
            _codeCreatorService = codeCreatorService;
        }

        /// <summary>
        /// 获取下拉框信息
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> ComboxDataAsync()
        {
            try
            {
                var dic = await _comboxDataService.Get(new Dictionary<string, object>()
                {
                    { "BCCode@#Auditing", "auditing" },
                    { "SpHouseName", (Expression<Func<SP_HOUSE, bool>>)null}
                });
                return AjaxResult.Success(dic);
            }
            catch (Exception e)
            {
                throw new Exception("获取下拉数据失败！原因：" + e.Message);
            }
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            var query = await _dbContext.Query<SP_RECEIVE>().GetGridData(request);
            return query;
        }

        /// <summary>
        /// 获取记录
        /// </summary>
        public async Task<SP_RECEIVE> GetAsync(string receiveId)
        {
            return await _dbContext.QueryByKeyAsync<SP_RECEIVE>(receiveId);
        }

        /// <summary>
        /// 同时保存主子表
        /// </summary>
        public async Task<AjaxResult> SaveAllAsync(SaveRequest<SP_RECEIVE> request, SaveRequest<SP_RECEIVE_DET> requestdet)
        {
            await _dbContext.UseTransactionAsync(async () => {
                request ??= new SaveRequest<SP_RECEIVE>();
                var execResult = await _dbContext.SaveEntityAnsyc(request,
                     c => new
                     {
                         c.AUDITING,
                         c.AUDITING_CHK,
                         c.RECEIVE_CODE,
                         c.RECEIVE_DATE,
                         c.USER_NAME,
                         c.PROVIDER_NAME,
                         c.ORDER_CODE,
                         c.ORDER_ID,
                         c.DEPT_NAME,
                         c.PUR_USER,
                         c.CHK_USER,
                         c.REG_MEMO,
                         c.MEMO,
                         c.RECEIVE_ID
                     },
                     c => a => a.RECEIVE_ID == c.RECEIVE_ID
                     , BeforeAdd, BeforeUpdate, BeforeDelete, false, null, null);

                requestdet ??= new SaveRequest<SP_RECEIVE_DET>();
                await _dbContext.SaveEntityAnsyc(requestdet,
                     c => new
                     {
                         c.SP_CODE,
                         c.SP_NAME,
                         c.UNIT,
                         c.SP_SIZE,
                         c.PRODUCE,
                         c.MEMO,
                         c.COUNT,
                         c.CHK_COUNT,
                         c.CHK_MONEY,
                         c.RETURN_MEMO,
                         c.DELIVERY_CODE,
                         c.STOCK_NAME,
                         c.STOCK_ID,
                         c.PRICE,
                         c.MONEY,
                         c.APPLY_USER,
                         c.APPLY_NO,
                         c.RECDET_ID,
                         c.ORDERDET_ID,
                         c.RECEIVE_ID
                     },
                     c => a => a.RECDET_ID == c.RECDET_ID, DetBeforAdd, null, null, false, null, null);
            });
            return AjaxResult.Success("保存成功");
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        private async Task BeforeAdd(SP_RECEIVE entity)
        {
            if (entity.RECEIVE_ID.IsNullOrWhiteSpace())
            {
                entity.RECEIVE_ID = GuidHelper.NewSnowflakeId().ToString();
            }
            if (entity.USER_ID.IsNullOrWhiteSpace())
            {
                entity.USER_ID = _userSession.UserID.ToString();
                entity.USER_NAME = _userSession.RealName;
            }
            if (entity.RECEIVE_CODE.IsNullOrWhiteSpace()) {
                entity.RECEIVE_CODE = await _codeCreatorService.CreateCodeAsync<SP_RECEIVE>("DJ", x => x.RECEIVE_CODE);
            }
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeDelete(SP_RECEIVE entity)
        {
            await _dbContext.DeleteAsync<SP_RECEIVE_DET>(x => x.RECEIVE_ID == entity.RECEIVE_ID);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(SP_RECEIVE entity)
        {
            entity.MODIFY_USERID = _userSession.UserID.ToString();
            entity.MODIFYDATE = await _dbContext.GetSysdate();
        }

        /// <summary>
        /// 明细新增前处理
        /// </summary>
        private async Task DetBeforAdd(SP_RECEIVE_DET entity)
        {
            if (entity.RECDET_ID.IsNullOrWhiteSpace())
            {
                entity.RECDET_ID = GuidHelper.NewSnowflakeId().ToString();
            }
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
                    var entity = await _dbContext.Query<SP_RECEIVE>(x => x.RECEIVE_ID == sid).FirstOrDefaultAsync();
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
        /// 撤销提交
        /// </summary>
        public async Task<AjaxResult> RevokeAsync(List<string> sids)
        {
            if (sids == null || sids.Count == 0) return AjaxResult.Error("请选择行");

            await _dbContext.UseTransactionAsync(async () =>
            {
                foreach (var sid in sids)
                {
                    var entity = await _dbContext.Query<SP_RECEIVE>(x => x.RECEIVE_ID == sid).FirstOrDefaultAsync();
                    if (entity == null) continue;
                    if (entity.AUDITING == "0")
                    {
                        throw new Exception("该数据未提交，无法撤销！");
                    }
                    if (entity.AUDITING_CHK == "1")
                    {
                        throw new Exception("该数据已验收，无法撤销！");
                    }

                    entity.AUDITING = "0";
                    await BeforeUpdate(entity);
                    await _dbContext.UpdateAsync(entity);
                }
            });
            return AjaxResult.Success("撤销成功");
        }

        /// <summary>
        /// 提交
        /// </summary>
        public async Task<AjaxResult> SubmitCheckAsync(List<string> sids)
        {
            if (sids == null || sids.Count == 0) return AjaxResult.Error("请选择行");

            await _dbContext.UseTransactionAsync(async () =>
            {
                foreach (var sid in sids)
                {
                    var entity = await _dbContext.Query<SP_RECEIVE>(x => x.RECEIVE_ID == sid).FirstOrDefaultAsync();
                    if (entity == null) continue;
                    if (entity.AUDITING_CHK == "1")
                    {
                        throw new Exception("该数据已提交，无法重复提交！");
                    }

                    entity.AUDITING_CHK = "1";
                    await BeforeUpdate(entity);
                    await _dbContext.UpdateAsync(entity);
                }
            });
            return AjaxResult.Success("提交成功");
        }

        /// <summary>
        /// 撤销提交
        /// </summary>
        public async Task<AjaxResult> RevokeCheckAsync(List<string> sids)
        {
            if (sids == null || sids.Count == 0) return AjaxResult.Error("请选择行");

            await _dbContext.UseTransactionAsync(async () =>
            {
                foreach (var sid in sids)
                {
                    var entity = await _dbContext.Query<SP_RECEIVE>(x => x.RECEIVE_ID == sid).FirstOrDefaultAsync();
                    if (entity == null) continue;
                    if (entity.AUDITING_CHK == "0")
                    {
                        throw new Exception("该数据未提交，无法撤销！");
                    }

                    entity.AUDITING_CHK = "0";
                    await BeforeUpdate(entity);
                    await _dbContext.UpdateAsync(entity);
                }
            });
            return AjaxResult.Success("撤销成功");
        }

        /// <summary>
        /// 获取列表
        /// </summary>
        public async Task<GridData> DetListAsync(GridRequest request)
        {
            return await _dbContext.JoinQuery<SP_RECEIVE_DET, SP_ORDER_DETAIL>((a, b) => new object[] {
                JoinType.LeftJoin,a.ORDERDET_ID==b.ORDERDET_ID
            }).Select((a, b) => new
            {
                b.STOP_NUM,
                DDCOUNT = b.COUNT,
                a.ORDERDET_ID,
                a.COUNT,
                DSCOUNT = b.COUNT - (b.STOP_NUM ?? 0) - (b.RECEIVE_COUNT2 ?? 0),
                a.PRICE,
                a.SP_CODE,
                a.SP_NAME,
                a.SP_SIZE,
                a.APPLY_NO,
                a.DELIVERY_CODE,
                a.STOCK_NAME,
                a.CHK_COUNT,
                a.RECDET_ID,
                a.STOCK_ID,
                a.APPLY_USER,
                a.DEPT_NAME,
                a.PRODUCE,
                a.UNIT,
                a.TYPE_NAME,
                a.MONEY,
                a.MEMO,
                a.RECEIVE_ID,
            })
            .GetGridData(request);
        }
    }
}
