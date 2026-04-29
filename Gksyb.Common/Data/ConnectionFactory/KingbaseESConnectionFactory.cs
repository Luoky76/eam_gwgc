using Chloe.Infrastructure;
using Chloe.KingbaseES;
using Chloe.RDBMS;
using Kdbndp;
using System.Data;

namespace Gksyb.Common.Data
{
    public class KingbaseESConnectionFactory : IDbConnectionFactory
    {
        static KingbaseESConnectionFactory()//初始化
        {
            var methodHandlerDic = new Dictionary<string, IMethodHandler>()
            {
                {IsNullOrWhiteSpace_Handler.MethodName, new IsNullOrWhiteSpace_Handler() },
                {Compare_Handler.MethodName, new Compare_Handler()},
                {Contains_Handler.MethodName, new Contains_Handler()},
                {SumString_Handler.MethodName, new SumString_Handler()},
                {MathAbs_Handler.MethodName, new MathAbs_Handler()},
                {MathRound_Handler.MethodName, new MathRound_Handler()},
                {MathFloor_Handler.MethodName, new MathFloor_Handler()},
                {MathCeiling_Handler.MethodName, new MathCeiling_Handler()}
            };
            foreach (var item in methodHandlerDic)
            {
                KingbaseESContext.SetMethodHandler(item.Key, item.Value);
            }
        }

        private readonly string _connString = null;

        public KingbaseESConnectionFactory(string connString)
        {
            this._connString = connString;
        }

        public IDbConnection CreateConnection()
        {
            IDbConnection conn = new KdbndpConnection(this._connString);
            return conn;
        }
    }
}