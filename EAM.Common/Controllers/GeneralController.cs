using Gksyb.Core.Auth;
using Microsoft.AspNetCore.Mvc;

namespace EAM.Common.Controllers
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
        public AjaxResult CreatePrimaryKey()
        {
            return AjaxResult.Success(GuidHelper.NewSnowflakeId().ToString(), "成功");
        }
    }
}
