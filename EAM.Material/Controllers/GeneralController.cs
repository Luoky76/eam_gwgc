using Gksyb.Core.Auth;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Material.Controllers
{
    [GksybAuthorize(true)]
    public class GeneralController : AreaController
    {
        public GeneralController()
        {
        }

        /// <summary>
        /// 生成主键
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost]
        public AjaxResult<string> CreatePrimaryKey()
        {
            return AjaxResult<string>.Success(GuidHelper.NewSnowflakeId().ToString(), "成功");
        }
    }
}