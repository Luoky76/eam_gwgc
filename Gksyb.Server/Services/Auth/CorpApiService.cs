using Gksyb.Core.Interfaces.Auth;
using Gksyb.Model.Core;
using Gksyb.Model.UI;
using System.Linq.Expressions;

namespace Gksyb.Server.Services.Auth
{
    public partial class CorpService
    {
        /// <inheritdoc/>
        public async Task<List<ComboxData>> ComboxDataAsync(bool isAll = false)
        {
            var data = await _dbContext.Query<CF_CORP>().WhereIf(!isAll, c => c.VALIDFLAG == "1").Select(c => new ComboxData()
            {
                ID = c.CORPID,
                TEXT = c.CNAME,
                VALUE = c.CORP_SNAME,
                FLAG = c.VALIDFLAG
            }).ToListAsync();
            return data.OrderBy(c => c.TEXT).ToList();
        }

        /// <inheritdoc/>
        public async Task<List<CorpInfo>> FindCorpsAsync(Expression<Func<CorpInfo, bool>> filter = null)
        {
            return await _dbContext.Query<CF_CORP>().Select(CorpInfoExtensions.SelectCorpInfo)
                .WhereIfNotNull(filter, filter).ToListAsync();
        }
    }
}