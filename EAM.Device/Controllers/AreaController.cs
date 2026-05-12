namespace Microsoft.AspNetCore.Mvc
{
    [Route("device")]
    [ApiController]
    public class AreaController<T> : BaseController<T>
    {
    }

    [Route("device")]
    [ApiController]
    public class AreaController : BaseController
    {
    }
}