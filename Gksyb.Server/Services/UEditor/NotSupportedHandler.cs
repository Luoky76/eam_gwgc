namespace Gksyb.Server.Services.UEditor
{
    public class NotSupportedHandler : Handler
    {
        public override object Process()
        {
            return new
            {
                state = "action 参数为空或者 action 不被支持。"
            };
        }
    }
}