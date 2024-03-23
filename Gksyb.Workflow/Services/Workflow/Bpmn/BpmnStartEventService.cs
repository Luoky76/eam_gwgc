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

        protected override async Task Exec()
        {
            if (_info.NodeStatus == NodeStatus.Back)//退回
            {
                await ExecBackAsync();
                return;
            }
            if (string.IsNullOrWhiteSpace(_info.TaskId))
            {
                await ExecNewAsync();
            }
            else
            {
                await ReExecAsync();
            }
        }

        /// <summary>
        /// 发起流程
        /// </summary>
        private async Task ExecNewAsync()
        {
            await AddWfTask();
            _info.Id = await AddTask();
            if (_info.NodeStatus == NodeStatus.Draft)
            {
                _info.ToNode = null;
                return;
            }
            await DoPostInterceptors();
            await Complate();
            await AddLog("发起");
        }

        /// <summary>
        /// 重新提交流程
        /// </summary>
        private async Task ReExecAsync()
        {
            await UpdateWfTask();
            if (_info.NodeStatus == NodeStatus.Draft)
            {
                _info.ToNode = null;
                return;
            }
            _info.NodeStatus ??= NodeStatus.Agree;
            await DoPostInterceptors();
            await Complate();
            await AddLog("重新提交");
            var nodes = await _dbContext.Query<WF_NODE>().Where(c => c.TASK_ID == _info.TaskId && c.NODE_STATUS == NodeStatus.BackArchived).ToListAsync();
            foreach (var node in nodes)
            {
                _dbContext.TrackEntity(node);
                node.NODE_STATUS = NodeStatus.Active;
                node.FINISHDATE = null;
                await _dbContext.UpdateAsync(node);
            }
            var nodeInfos = nodes.Select(c => c.ToNodeInfo()).ToList();
            _info.ToDos.AddRange(nodeInfos);
            _info.ToNode = null;
        }

        /// <summary>
        /// 退回等引起的重新进入
        /// </summary>
        private async Task ExecBackAsync()
        {
            _info.Users = await _dbContext.Query<WF_TASK>().Where(c => c.ID == _info.TaskId).Select(c => new UserInfo
            {
                Id = c.CREATEUSERID,
                Account = c.CREATEUSERNAME,
                Name = c.CREATEUSER
            }).ToListAsync();
            MessageException.ThrowIf(_info.Users.Count < 1, $"找不到{_info.TaskId}的任务");
            await AddTask();
        }

        /// <summary>
        /// 添加流程任务
        /// </summary>
        private async Task AddWfTask()
        {
            var id = GuidHelper.NewShortId();
            var company = _info.CorpId;
            if (!string.IsNullOrWhiteSpace(company))
            {
                var service = _serviceProvider.GetService<ICorpService>();
                var corpInfo = await service.ParentCompany(_info.CorpId);
                if (corpInfo != null) company = corpInfo.CorpID;
            }
            var entity = new WF_TASK()
            {
                ID = id,
                FLOW_ID = _info.FlowId,
                FLOW_NAME = _info.FlowName,
                FLOW_TITLE = BuildTitle(),
                TASK_KEY = _info.GetTaskKey(id),
                FLOW_FORM_DATA = _info.FormData.ToJson(),
                FLOW_STATUS = WF_TASKExtensions.Active,
                COMPANY = company,
                CORPID = _info.CorpId,
                CREATEUSERID = User.UserID,
                CREATEUSERNAME = User.UserName,
                CREATEUSER = User.RealName,
                CREATEDATE = await _dbContext.GetSysdate(),
                APPNAME = _info.AppName
            };
            await _dbContext.InsertAsync(entity);
            _info.TaskId = entity.ID;
            _info.Creator = entity.CREATEUSER;
            _info.CreateDate = entity.CREATEDATE;
            _info.Users = new List<UserInfo>(){
                new() {
                    Id= User.UserID,
                    Account = User.UserName,
                    Name= User.RealName
                }
            };
        }

        /// <summary>
        /// 更新流程任务
        /// </summary>
        private async Task UpdateWfTask()
        {
            var formData = _info.FormData.ToJson();
            var title = BuildTitle();
            await _dbContext.UpdateAsync<WF_TASK>(c => c.ID == _info.TaskId && c.FLOW_STATUS == WF_TASKExtensions.Active, c => new WF_TASK()
            {
                FLOW_FORM_DATA = formData,
                FLOW_TITLE = title
            });
        }

        private string BuildTitle()
        {
            _info.RealTitle = _info.Title.Replace(null, _info.FormData, FilterParmMatch.CurrentParmMatch);
            return _info.RealTitle;
        }
    }
}