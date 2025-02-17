using Chloe.Entity;
using Gksyb.Common.Static;
using Gksyb.Model.WorkFlow;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Gksyb.Workflow.Data
{
    public class WfFlowMap : InternalEntityTypeBuilder<WF_FLOW>
    {
        private static string _appanme;

        private static string Appname
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_appanme))
                {
                    var options = HttpContext.RequestServices.GetService<IOptions<SysContextOptions>>()?.Value;
                    _appanme = options.WorkflowAppName ?? options.AppName;
                }
                return _appanme;
            }
        }

        public WfFlowMap()
        {
            HasQueryFilter(() =>
            {
                return c => c.APPNAME == Appname;
            });
        }
    }
}