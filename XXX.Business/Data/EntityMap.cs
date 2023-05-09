using Chloe.Entity;
using Gksyb.Model.Core;
using Gksyb.Model.XXX.Business;

namespace Gksyb.Server.Data
{
    public class SampleTableMap : InternalEntityTypeBuilder<SAMPLE_TABLE>
    {
        public SampleTableMap()
        {
            HasQueryFilter(() =>
            {
                return c => c.RECORDSTATUS != Oper.Delete;
                //var user = HttpContext.Current?.GetCurrentUserAsync().GetResult();
                //if (user == null) return c => c.RECORDSTATUS != Oper.Delete;
            });
        }
    }
}