using Microsoft.AspNetCore.Http;

namespace Gksyb.Server.Services.UEditor
{
    public abstract class Handler
    {
        public Handler()
        {
            var context = Gksyb.Common.Static.HttpContext.Current;
            this.Request = context.Request;
            this.Response = context.Response;
            this.Context = context;
        }

        public abstract object Process();

        public HttpRequest Request { get; private set; }
        public HttpResponse Response { get; private set; }
        public HttpContext Context { get; private set; }
    }
}