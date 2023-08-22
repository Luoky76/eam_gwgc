using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Grid;
using Gksyb.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAM.Material.Services
{
    public class SpInBackService
    {
        private readonly IDbContext _dbContext;
        private readonly IComboxDataService _comboxDataService;

        public SpInBackService(IDbContext dbContext, IComboxDataService comboxDataService)
        {
            _dbContext = dbContext;
            _comboxDataService = comboxDataService;
        }

        public async Task<GridData> ListAsync(GridRequest request)
        {
            var query = await _dbContext.Query<SP_IN_BACK>().GetGridData(request);
            return query;
        }

        public async Task<AjaxResult> GetAsync(string ID)
        {
            var query = await _dbContext.Query<SP_IN_BACK>().Where(x => x.IN_BACK_ID == ID).ToListAsync();

            return AjaxResult.Success(query);
        }
    }
}
