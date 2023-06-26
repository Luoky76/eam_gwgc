namespace Microsoft.AspNetCore.Mvc
{
    [Route("special")]
    [ApiController]
    public class AreaController<T> : BaseController<T>
    {
    }

    [Route("special")]
    [ApiController]
    public class AreaController : BaseController
    {
    }
}