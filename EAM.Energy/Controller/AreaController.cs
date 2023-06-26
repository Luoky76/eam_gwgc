namespace Microsoft.AspNetCore.Mvc
{
    [Route("energy")]
    [ApiController]
    public class AreaController<T> : BaseController<T>
    {
    }

    [Route("energy")]
    [ApiController]
    public class AreaController : BaseController
    {
    }
}