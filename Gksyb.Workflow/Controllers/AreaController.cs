using Microsoft.AspNetCore.Mvc;

namespace Gksyb.Workflow.Controllers
{
    [Route("Workflow")]
    [ApiController]
    public class AreaController<T> : BaseController<T>
    {
    }

    [Route("Workflow")]
    [ApiController]
    public class AreaController : BaseController
    {
    }
}