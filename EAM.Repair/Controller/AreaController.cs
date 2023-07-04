namespace Microsoft.AspNetCore.Mvc
{
    [Route("repair")]
    [ApiController]
    public class AreaController<T> : BaseController<T>
    {
    }

    [Route("repair")]
    [ApiController]
    public class AreaController : BaseController
    {
    }
}