using Gksyb.Common;
using Gksyb.Core.Application;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Model;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Server.Interfaces.Auth;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gksyb.Server.Services.Auth
{
    public class SelectUserService : BaseService<CF_CORP>, ISelectUserService,IBaseService
    {
        private readonly UserSession CurrentUser;

        public SelectUserService(IDbContext dbContext, UserSession userSession) : base(dbContext)
        {
            CurrentUser = userSession;
        }

        public async Task<GridData> GetCurentGorpUserList(GridRequest request)
        {
            var corpId = CurrentUser.Corp.CorpID;
            //var corpId = CurrentUser.BusiCompany[0];

            var list = await _dbContext.Query<CF_USER>()
                .LeftJoin<CF_USER_PORT>((a, b) => a.LOGINNAME == b.LOGINNAME)
                .LeftJoin<CF_DEPT>((a, b, c) => c.CORPID == b.CORPID)
                .LeftJoin<CF_CORP>((a, b, c, d) => c.CORPID == d.CORPID)
                .Select((a, b, c, d) => new
               {
                   a.LOGINNAME,
                   a.REALNAME,
                   c.DEPT_NAME,
                   c.CORPID,
                   d.CORP_SNAME,
                   c.DEPT_ID,
                })
                .Where(c => c.CORPID == corpId)
                .GetGridData(request);
            return list;
        }
    }
}
