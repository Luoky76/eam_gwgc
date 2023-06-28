using Gksyb.Common;
using Gksyb.Core.Application;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Gksyb.Core.Interfaces.Common
{
    public interface ISystemPermissionsCommonService : IService
    {
        public Task<AjaxResult> GetCurrentCorp();
        /// <summary>
        /// 公司数组如["8003","8002"]
        /// </summary>
        /// <returns></returns>
        public Task<List<string>> GetCompanyList();
        /// <summary>
        /// 带前后点的数组,如[",8003,",",8002,"]
        /// </summary>
        /// <returns></returns>
        public Task<List<string>> GetCompanyListContainSpot();

        public Task<List<string>> GetCompanyListContainSpot(string dept);

        public Task<List<ComboxData>> GetCompanyCombox();
    }
}
