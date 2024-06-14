using Gksyb.Common;
using Gksyb.Model;
using Gksyb.Model.Grid;

namespace EAM.Special.Interfaces
{
    public interface IDrugCollectService : IService
    {
        /// <summary>
        /// 获取列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<GridData> ListAsync(GridRequest request);

        /// <summary>
        /// 根据ID获取单行记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Task<DRUG_COLLECT> GetAsync(string id);

        /// <summary>
        /// 生成主键
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        public string CreatePrimaryKey();

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<AjaxResult> SaveAsync(SaveRequest<DRUG_COLLECT> request);

        /// <summary>
        /// 同时保存采购主表以及与其关联的采购明细子表、采购需求子表
        /// </summary>
        /// <param name="request1"></param>
        /// <param name="request2"></param>
        /// <param name="request3"></param>
        /// <returns></returns>
        public Task<AjaxResult> SaveAllAsync
            (SaveRequest<DRUG_COLLECT> request1, SaveRequest<DRUG_COLLECT_DET> request2, SaveRequest<DRUG_COLLECT_REQUEST> request3);

        /// <summary>
        /// 获取下拉框数据
        /// </summary>
        public Task<AjaxResult> ComboxData();
    }
}
