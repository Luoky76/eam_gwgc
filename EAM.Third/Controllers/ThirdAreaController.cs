namespace Microsoft.AspNetCore.Mvc
{
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