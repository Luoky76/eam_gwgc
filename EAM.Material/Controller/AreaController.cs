namespace Microsoft.AspNetCore.Mvc
{
    [Route("material")]
    [ApiController]
    public class AreaController<T> : BaseController<T>
    {
    }

    [Route("material")]
    [ApiController]
    public class AreaController : BaseController
    {
    }
}