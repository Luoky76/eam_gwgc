using Gksyb.Core.Application;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;

namespace Gksyb.Server.Services.Common
{
    /// <summary>
    /// 字典服务
    /// </summary>
    public class BcCodeService : BaseService<BC_CODE>, IBCCodeService
    {
        /// <summary>
        /// 字典服务
        /// </summary>
        public BcCodeService(IDbContext dbContext) : base(dbContext)
        {
        }

        /// <summary>
        /// 根据类型获取字典
        /// </summary>
        /// <returns></returns>
        public async Task<List<ComboxData>> Get(string codeType, string code = null)
        {
            using var dbContext = _dbContext.Clone();
            return await dbContext.Query<BC_CODE>().Where(c => c.CODE_TYPE == codeType)
                .WhereIfNotNullOrEmpty(code, c => c.CODE_EN == code)
                .OrderBy(c => c.CODE_SEQ)
                .Select(c => new ComboxData() { ID = c.CODE_EN, TEXT = c.CODE_CN, VALUE = c.REMARK, FLAG = c.CODE_SEQ })
               .ToListAsync();
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override async Task<AjaxResult> SaveAsync(SaveRequest<BC_CODE> request)
        {
            return await _dbContext.SaveEntityAnsyc(request,
                c => new { c.CODE_TYPE, c.CODE_CN, c.CODE_EN, c.CODE_SEQ, c.REMARK },
                c => a => a.SID == c.SID, BeforeAdd);
        }

        /// <summary>
        /// 新增前
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private async Task BeforeAdd(BC_CODE entity)
        {
            entity.SID = GuidHelper.NewShortId();
            await Task.CompletedTask;
        }
    }
}