using Gksyb.Core.Filter;
using Gksyb.Core.Interfaces.Auth;
using Gksyb.Core.Interfaces.WorkFlow;
using Gksyb.Model.WorkFlow;
using Microsoft.Extensions.DependencyInjection;

namespace Gksyb.Workflow.Services.Workflow.Bpmn
{
    [ServiceLifetime]
    public class BpmnStartEventService : BpmnNodeService, IBaseService
    {
        public BpmnStartEventService(IDbContext dbContext, IServiceProvider serviceProvider) : base(dbContext, serviceProvider)
        {
        }

        protected override async Task Exec(FlowExecuteInfo info)
        {
            if (info.NodeStatus == NodeStatus.Back)//退回
            {
                await ExecBackAsync(info);
                return;
            }
            if (string.IsNullOrWhiteSpace(info.TaskId))
            {
                await ExecNewAsync(info);
            }
            else
            {
                await ReExecAsync(info);
            }
        }

        /// <summary>
        /// 发起流程
        /// </summary>
        private async Task ExecNewAsync(FlowExecuteInfo info)
        {
            await AddWfTask(info);
            info.Id = await AddTask(info, false);
            if (info.NodeStatus == NodeStatus.Draft)
            {
                info.ToNode = null;
                return;
            }
            await Complate(info);
            await AddLog(info, "发起");
        }

        /// <summary>
        /// 重新提交流程
        /// </summary>
        private async Task ReExecAsync(FlowExecuteInfo info)
        {
            await UpdateWfTask(info);
            if (info.NodeStatus == NodeStatus.Draft)
            {
                info.ToNode = null;
                return;
            }
            info.NodeStatus ??= NodeStatus.Agree;
            await Complate(info);
            await AddLog(info, "重新提交");
            await _dbContext.UpdateAsync<WF_NODE>(c => c.TASK_ID == info.TaskId && c.NODE_STATUS == NodeStatus.BackArchived, c => new WF_NODE()
            {
                NODE_STATUS = NodeStatus.Active,
                FINISHDATE = null
            });
            info.ToNode = null;
        }

        /// <summary>
        /// 退回等引起的重新进入
        /// </summary>
        private async Task ExecBackAsync(FlowExecuteInfo info)
        {
            info.Users = await _dbContext.Query<WF_TASK>().Where(c => c.ID == info.TaskId).Select(c => new UserInfo
            {
                Id = c.CREATEUSERID,
                Account = c.CREATEUSERNAME,
                Name = c.CREATEUSER
            }).ToListAsync();
            MessageException.ThrowIf(info.Users.Count < 1, $"找不到{info.TaskId}的任务");
            await AddTask(info);
        }

        /// <summary>
        /// 添加流程任务
        /// </summary>
        private async Task AddWfTask(FlowExecuteInfo info)
        {
            var id = GuidHelper.NewShortId();
            var company = info.CorpId;
            if (!string.IsNullOrWhiteSpace(company))
            {
                var service = _serviceProvider.GetService<ICorpService>();
                var corpInfo = await service.ParentCompany(info.CorpId);
                if (corpInfo != null) company = corpInfo.CorpID;
            }
            var entity = new WF_TASK()
            {
                ID = id,
                FLOW_ID = info.FlowId,
                FLOW_NAME = info.FlowName,
                FLOW_TITLE = BuildTitle(info),
                TASK_KEY = info.GetTaskKey(id),
                FLOW_FORM_DATA = info.FormData.ToJson(),
                FLOW_STATUS = WF_TASKExtensions.Active,
                COMPANY = company,
                CORPID = info.CorpId,
                CREATEUSERID = User.UserID,
                CREATEUSERNAME = User.UserName,
                CREATEUSER = User.RealName,
                CREATEDATE = await _dbContext.GetSysdate(),
                APPNAME = info.AppName
            };
            await _dbContext.InsertAsync(entity);
            info.TaskId = entity.ID;
            info.Users = new List<UserInfo>(){
                new UserInfo
                {
                    Id= User.UserID,
                    Account = User.UserName,
                    Name= User.RealName
                }
            };
        }

        /// <summary>
        /// 更新流程任务
        /// </summary>
        private async Task UpdateWfTask(FlowExecuteInfo info)
        {
            var formData = info.FormData.ToJson();
            var title = BuildTitle(info);
            await _dbContext.UpdateAsync<WF_TASK>(c => c.ID == info.TaskId && c.FLOW_STATUS == WF_TASKExtensions.Active, c => new WF_TASK()
            {
                FLOW_FORM_DATA = formData,
                FLOW_TITLE = title
            });
        }

        private static string BuildTitle(FlowExecuteInfo info) => info.Title.Replace(null, info.FormData, FilterParmMatch.CurrentParmMatch);
    }
}