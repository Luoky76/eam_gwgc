namespace Microsoft.AspNetCore.Mvc
{
    [Route("Third")]
    [ApiController]
    public class AreaController<T> : BaseController<T>
    {
    }

    [Route("Third")]
    [ApiController]
    public class AreaController : BaseController
    {
    }
}