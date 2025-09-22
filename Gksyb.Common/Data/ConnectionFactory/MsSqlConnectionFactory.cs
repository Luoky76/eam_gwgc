using Chloe.RDBMS;
using Chloe.SqlServer;

namespace Gksyb.Common.Data
{
    public class MsSqlConnectionFactory : DefaultDbConnectionFactory
    {
        static MsSqlConnectionFactory()//初始化
        {
            var methodHandlerDic = new Dictionary<string, IMethodHandler>()
            {
                {IsNullOrWhiteSpace_Handler.MethodName, new IsNullOrWhiteSpace_Handler() },
                {Compare_Handler.MethodName, new Compare_Handler()},
                {Contains_Handler.MethodName, new Contains_Handler("+")},
                {SumString_Handler.MethodName, new SumString_Handler(dis:"")},
                {MathAbs_Handler.MethodName, new MathAbs_Handler()},
                {MathRound_Handler.MethodName, new MathRound_Handler()},
                {MathFloor_Handler.MethodName, new MathFloor_Handler()},
                {MathCeiling_Handler.MethodName, new MathCeiling_Handler("CEILING")}
            };
            foreach (var item in methodHandlerDic)
            {
                MsSqlContext.SetMethodHandler(item.Key, item.Value);
            }
        }

        public MsSqlConnectionFactory(string connString) : base(connString)
        {
        }
    }
}