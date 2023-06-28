namespace Microsoft.AspNetCore.Mvc
{
    [Route("Material")]
    [ApiController]
    public class AreaController<T> : BaseController<T>
    {
    }

    [Route("Material")]
    [ApiController]
    public class AreaController : BaseController
    {
    }
}