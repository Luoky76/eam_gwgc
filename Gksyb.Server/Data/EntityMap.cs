using Chloe.Entity;
using Gksyb.Model.Core;

namespace Gksyb.Server.Data
{
    public class CorpMap : InternalEntityTypeBuilder<CF_CORP>
    {
        public CorpMap()
        {
            HasQueryFilter(() =>
            {
                return c => c.RECORDSTATUS != Oper.Delete;
                //var user = HttpContext.Current?.GetCurrentUserAsync().GetResult();
                //if (user == null) return c => c.RECORDSTATUS != Oper.Delete;
            });
        }
    }

    public class UserMap : InternalEntityTypeBuilder<CF_USER>
    {
        public UserMap()
        {
            HasQueryFilter(() =>
            {
                return c => c.RECORDSTATUS != Oper.Delete;
            });
        }
    }

    public class MessageTemplateMap : InternalEntityTypeBuilder<SYS_MESSAGE_TEMPLATE>
    {
        public MessageTemplateMap()
        {
            HasQueryFilter(() =>
            {
                return c => c.FLAG == "1";
            });
        }
    }
}