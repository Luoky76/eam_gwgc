using Gksyb.Model.Grid;
using Gksyb.Model;
using Gksyb.Model.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAM.Material.Interfaces
{
    public interface ISpOutService : IService
    {
        /// <summary>
        /// 获取下拉
        /// </summary>
        /// <returns></returns>
        public Task<ConcurrentDictionary<string, List<ComboxData>>> ComboxData();

        /// <summary>
        /// 获取物料领用申请记录
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetSpOutAppList(GridRequest request);

        /// <summary>
        /// 获取单条物料领用申请记录
        /// </summary>
        /// <returns></returns>
        Task<SP_OUT_APP> GetSpOutAppListDetail(string ID);

        /// <summary>
        /// 管理物料领用申请记录
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManageSpOutApp(SaveRequest<SP_OUT_APP> request, SaveRequest<SP_OUTAPP_DET> requestdet);

        /// <summary>
        /// 提交物料领用申请
        /// </summary>
        /// <returns></returns>
        public Task<int> SubmitSpOutApp(string sid);

        /// <summary>
        /// 获取申请明细
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetSpOutAppdetList(GridRequest request);
    }
}
