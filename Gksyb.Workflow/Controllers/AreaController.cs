namespace Microsoft.AspNetCore.Mvc
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