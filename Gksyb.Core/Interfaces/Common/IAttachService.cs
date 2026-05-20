using Gksyb.Core.Auth;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Http;

namespace Gksyb.Core.Interfaces.Common
{
    public interface IAttachService : IService
    {
        /// <summary>
        /// 获取缓存用户
        /// </summary>
        /// <param name="user"></param>
        void SetUser(UserSession user);

        /// <summary>
        /// 上传附件
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        Task<AjaxResult> Upload([FileOptions("gif,jpg,jpeg,bmp,png,pdf,xlsx,xls,doc,docx", 200)] IFormFile formFile, string parms);

        /// <summary>
        /// 获取某单据关联的各类附件数量
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dataId"></param>
        /// <returns></returns>
        Task<AjaxResult> GetAttachFieldStat(string tableName, string dataId);

        /// <summary>
        /// 删除附件（根据附件ID）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<AjaxResult> DeleteAttachByAttachId(string id);

        /// <summary>
        /// 获取附件列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<GridData> ListAsync(GridRequest request);

    }
}