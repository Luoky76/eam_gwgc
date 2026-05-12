namespace Microsoft.AspNetCore.Mvc
{
    [Route("Common")]
    [ApiController]
    public class AreaController<T> : BaseController<T>
    {
    }

    [Route("Common")]
    [ApiController]
    public class AreaController : BaseController
    {
    }
}