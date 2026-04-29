using Chloe.Dameng;
using Chloe.Infrastructure;
using Chloe.RDBMS;
using Dm;
using System.Data;

namespace Gksyb.Common.Data
{
    public class DamengConnectionFactory : IDbConnectionFactory
    {
        static DamengConnectionFactory()//初始化
        {
            var methodHandlerDic = new Dictionary<string, IMethodHandler>()
            {
                {IsNullOrWhiteSpace_Handler.MethodName, new IsNullOrWhiteSpace_Handler() },
                {Compare_Handler.MethodName, new Compare_Handler()},
                {Contains_Handler.MethodName, new Contains_Handler()},
                {SumString_Handler.MethodName, new SumString_Handler("LISTAGG")},
                {MathAbs_Handler.MethodName, new MathAbs_Handler()},
                {MathRound_Handler.MethodName, new MathRound_Handler()},
                {MathFloor_Handler.MethodName, new MathFloor_Handler()},
                {MathCeiling_Handler.MethodName, new MathCeiling_Handler()}
            };
            foreach (var item in methodHandlerDic)
            {
                DamengContext.SetMethodHandler(item.Key, item.Value);
            }
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