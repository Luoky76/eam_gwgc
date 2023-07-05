using Gksyb.Core.Application;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gksyb.Server.Interfaces.Auth
{
    public interface ISelectUserService : IService<CF_CORP>
    {

        /// <summary>
        /// 获取当前部门的所有人员(staffNo)
        /// </summary>
        /// <returns></returns>
        Task<GridData> GetCurentGorpUserList(GridRequest request);

    }
}
