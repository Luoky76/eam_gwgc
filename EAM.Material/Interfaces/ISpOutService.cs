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
        /// 导入功能
        /// </summary>
        /// <returns></returns>
        public Task<GridData> ImportSpList(GridRequest request);

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

        /// <summary>
        /// 获取物料领用出库记录
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetSpOutStoreList(GridRequest request);

        /// <summary>
        /// 获取单条物料领用出库记录
        /// </summary>
        /// <returns></returns>
        Task<SP_OUTSTORE> GetSpOutStoreListDetail(string ID);

        /// <summary>
        /// 管理物料领用出库记录
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManageSpOutStore(SaveRequest<SP_OUTSTORE> request, SaveRequest<SP_OUTSTORE_DET> requestdet);

        /// <summary>
        /// 提交物料领用出库
        /// </summary>
        /// <returns></returns>
        public Task<int> SubmitSpOutStore(string sid);

        /// <summary>
        /// 注销物料领用出库
        /// </summary>
        /// <returns></returns>
        public Task<int> UnSubmitSpOutStore(string sid);

        /// <summary>
        /// 获取出库明细
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetSpOutStoredetList(GridRequest request);

        /// <summary>
        /// 获取出库冲红记录
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetSpOutBackList(GridRequest request);

        /// <summary>
        /// 获取单条出库冲红记录
        /// </summary>
        /// <returns></returns>
        Task<SP_OUT_BACK> GetSpOutBackListDetail(string ID);
        /// <summary>
        /// 管理出库冲红记录
        /// </summary>
        /// <returns></returns>
        public Task<AjaxResult> ManageSpOutBack(List<SP_OUTSTORE> request);
        /// <summary>
        /// 提交
        /// </summary>
        /// <returns></returns>
        public Task<int> SubmitSpOutBack(string sid);

        /// <summary>
        /// 导入功能
        /// </summary>
        /// <returns></returns>
        public Task<GridData> ImportList(GridRequest request);

        /// <summary>
        /// 获取物料出库明细记录
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetSpOutStoreDetailList(GridRequest request);

        /// <summary>
        /// 获取物料冲红明细记录
        /// </summary>
        /// <returns></returns>
        public Task<GridData> GetSpOutBackDetailList(GridRequest request);
    }
}
