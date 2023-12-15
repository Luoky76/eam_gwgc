namespace Microsoft.AspNetCore.Mvc
{
    [Route("Third")]
    [ApiController]
    public class ThirdAreaController<T> : BaseController<T>
    {
    }

    [Route("Third")]
    [ApiController]
    public class ThirdAreaController : BaseController
    {
    }
}