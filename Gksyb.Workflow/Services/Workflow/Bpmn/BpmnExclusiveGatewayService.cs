using Gksyb.Core.Interfaces.WorkFlow;

namespace Gksyb.Workflow.Services.Workflow.Bpmn
{
    [ServiceLifetime]
    public class BpmnExclusiveGatewayService : BpmnNodeService, IBaseService
    {
        public BpmnExclusiveGatewayService(IDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
        {
        }

        /// <summary>
        /// 变迁的条件表达式
        /// </summary>
        private string Expression
        {
            get
            {
                return GetProperties("expr") as string;
            }
        }

        /// <summary>
        /// 变迁的处理类
        /// </summary>
        private string Handle
        {
            get
            {
                return GetProperties("handle") as string;
            }
        }

        protected override async Task Exec()
        {
            var expression = Expression;
            var outputs = Outputs;
            if (_info.FormData == null)
            {
                await SetFormData();
            }
            var handle = GetHandle();
            var result = handle == null ? null : await handle.Eval(_info);
            if (!string.IsNullOrWhiteSpace(expression))
            {
                result = Eval(expression, _info.FormData, _info.TaskId, GetPreviousNodeNames(Inputs));
            }
            if (Outputs.Count < 2 && result == "False") return;
            if (result != "True") outputs = outputs.Where(c => c.Name.EqualsTo(result)).ToList();
            MessageException.ThrowIf(outputs.Count < 1 && !string.IsNullOrWhiteSpace(result), $"找不到连线{result}");
            await outputs.ForEachAsync(async c => await c.Execute());
        }

        public override async Task Complate()
        {
            await Task.CompletedTask;
            MessageException.Throw($"分支节点{Title}不能被完成");
        }

        /// <summary>
        /// 根据顺序，获取拦截器
        /// </summary>
        private IFlowGatewayHandle GetHandle()
        {
            var handle = Handle;
            if (string.IsNullOrWhiteSpace(handle)) return null;
            return _serviceProvider.GetService(a => (a.ImplementationType ?? a.ServiceType).FullName == handle) as IFlowGatewayHandle;
        }
    }
}