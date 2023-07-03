using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Model;
using Gksyb.Model.Grid;
using Gksyb.Model.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAM.Device.interfaces
{
    public interface IDeviceRunService : IService
    {
        /// <summary>
        /// 获取下拉
        /// </summary>
        /// <returns></returns>
        public Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData();
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetRun(GridRequest request);

        /// <summary>
        /// 增删改
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> Manage(SaveRequest<RUN_TRANS> request);

        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> Submit(string sids, string deid, string newStatus);

        /// <summary>
        /// 获取运行状态一览表
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetAllRun(GridRequest request);

    }
}
