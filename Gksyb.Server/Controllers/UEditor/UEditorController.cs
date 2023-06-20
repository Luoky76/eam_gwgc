using Gksyb.Core.Auth;
using Gksyb.Server.Services.UEditor;
using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Server.Controllers.UEditor
{
    /// <summary>
    /// 编辑器处理
    /// </summary>
    [GksybAuthorize(true)]
    public class UEditorController : ControllerBase
    {
        [HttpGet, HttpPost, DisableRequestSizeLimit, RequestFormLimits(MultipartBodyLengthLimit = long.MaxValue, ValueLengthLimit = int.MaxValue)]
        public ContentResult Process(string callback)
        {
            string json;
            try
            {
                var handle = HandelFactory.GetHandler(HttpContext);
                json = handle.Process().ToJson();
            }
            catch (Exception ex)
            {
                json = (new
                {
                    state = ex.ToString()
                }).ToJson();
            }
            if (string.IsNullOrWhiteSpace(callback))
            {
                return Content(json, "text/plain", Encoding.UTF8);
            }
            return Content($"{callback}({json});", "application/javascript", Encoding.UTF8);
        }
    }
}