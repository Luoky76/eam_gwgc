namespace Microsoft.AspNetCore.Mvc
{
    [Route("Business")]
    [ApiController]
    public class AreaController<T> : BaseController<T>
    {
    }

    [Route("Business")]
    [ApiController]
    public class AreaController : BaseController
    {
    }
}