using Chloe.Dameng;
using Chloe.Infrastructure;
using Dm;
using System.Data;

namespace Gksyb.Common.Data
{
    public class DamengConnectionFactory : IDbConnectionFactory
    {
        static DamengConnectionFactory()//初始化
        {
            DamengContext.SetMethodHandler(IsNullOrWhiteSpace_Handler.MethodName, new IsNullOrWhiteSpace_Handler());
            DamengContext.SetMethodHandler(Compare_Handler.MethodName, new Compare_Handler());
        }

        private readonly string _connString = null;

        public DamengConnectionFactory(string connString)
        {
            this._connString = connString;
        }

        public IDbConnection CreateConnection()
        {
            IDbConnection conn = new DmConnection(this._connString);
            return conn;
        }
    }
}