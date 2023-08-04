using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Grid;
using Gksyb.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gksyb.Core.Grid;
using EAM.Material.Interfaces;

namespace EAM.Material.Services
{
    public class SpReceiveService : ISpReceiveService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;

        public SpReceiveService(IDbContext dbContext, IComboxDataService comboxDataService)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
        }

        public async Task<GridData> ListAsync(GridRequest request)
        {
            var query = await _dbContext.Query<SP_RECEIVE>().GetGridData(request);
            return query;
        }

        public async Task<AjaxResult> GetAsync(string ID)
        {
            var query = await _dbContext.Query<SP_RECEIVE>().Where(x => x.RECEIVE_ID == ID).ToListAsync();

            return AjaxResult.Success(query);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> Save(SaveRequest<SP_RECEIVE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new
                {
                    c.AUDITING,
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
                    c.RECEIVE_ID
                },
                c => a => a.RECEIVE_ID == c.RECEIVE_ID, BeforeAdd, BeforeUpdate, BeforeDelete, false);
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        private async Task BeforeAdd(SP_RECEIVE entity)
        {
            entity.RECEIVE_ID = GuidHelper.NewSnowflakeId().ToString();

            string type = "DJ" + DateTime.Now.ToString("yyyyMM");
            string def = type + "000000";
            var model = await _dbContext.Query<SP_RECEIVE>(x => x.RECEIVE_CODE.Contains(type)).Select(x => Sql.Max(x.RECEIVE_CODE) ?? def).FirstOrDefaultAsync();
            var index = model.SubStr(8, 6).CastTo<int>() + 1;
            entity.RECEIVE_CODE = type + index.ToString("D6");

            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeUpdate(SP_RECEIVE request)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeDelete(SP_RECEIVE request)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 采购订单列表
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> OrderList()
        {
            var result = await _dbContext.JoinQuery<SP_ORDER, SP_ORDER_DETAIL>((a, b) => new object[]
               {
                   JoinType.LeftJoin,a.ORDER_ID.Equals(b.ORDER_ID)
               })
               .Select((a,b)=> new { a.ORDER_CODE,b.SP_CODE, b.SP_NAME , b.APPLY_USER ,a.BUY_USER, a.PROVIDER_NAME })
               .ToListAsync();
            return AjaxResult.Success(result, "成功");
        }

        public async Task<GridData> DetListAsync(GridRequest request)
        {
            var query = await _dbContext.JoinQuery<SP_RECEIVE, SP_RECEIVE_DET>((a, b) => new object[] {
                JoinType.LeftJoin,a.RECEIVE_ID .Equals(b.RECEIVE_ID )
            }).Select((a, b) => new
            {
                b.SP_CODE,
                b.SP_NAME,
                b.SP_SIZE,
                b.PRODUCE,
                b.UNIT,
                b.COUNT,
                b.PRICE,
                b.MONEY,
                b.APPLY_USER,
                b.MEMO,
                b.DELIVERY_CODE,
                b.RECDET_ID,
            }).GetGridData(request);

            return query;
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <returns></returns>
        public async Task<AjaxResult> SaveDet(SaveRequest<SP_RECEIVE_DET> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                a => new
                {
                    a.SP_CODE,
                    a.SP_NAME,
                    a.SP_SIZE,
                    a.PRODUCE,
                    a.UNIT,
                    a.COUNT,
                    a.PRICE,
                    a.MONEY,
                    a.APPLY_USER,
                    a.MEMO,
                    a.DELIVERY_CODE,
                    a.RECDET_ID,
                },
                c => a => a.RECDET_ID == c.RECDET_ID, BeforeAddItem, BeforeUpdateItem, BeforeDeleteItem, false);
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        private async Task BeforeAddItem(SP_RECEIVE_DET entity)
        {
            entity.RECDET_ID = GuidHelper.NewSnowflakeId().ToString();

            await Task.CompletedTask;
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeUpdateItem(SP_RECEIVE_DET request)
        {
            await Task.CompletedTask;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task BeforeDeleteItem(SP_RECEIVE_DET request)
        {
            await Task.CompletedTask;
        }
    }
}
