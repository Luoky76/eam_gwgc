using Chloe.SqlServer;

namespace Gksyb.Common.Data
{
    public class MsSqlConnectionFactory : DefaultDbConnectionFactory
    {
        static MsSqlConnectionFactory()//初始化
        {
            MsSqlContext.SetMethodHandler(IsNullOrWhiteSpace_Handler.MethodName, new IsNullOrWhiteSpace_Handler());
        }

        public MsSqlConnectionFactory(string connString) : base(connString)
        {
        }
    }
}