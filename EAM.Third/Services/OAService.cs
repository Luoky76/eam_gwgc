using Gksyb.Core.Auth;
using Gksyb.Core.Application;
using Gksyb.Core.Interfaces.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using Gksyb.Model;
using Gksyb.Core.Interfaces.Material;
using Gksyb.Core.Interfaces.Repair;

namespace EAM.Third.Services
{
    public class OAService: BaseService
    {
        private IDbContext _dbContext;
        private readonly UserSession _userSession;
        private readonly IMessageCenterService _messageCenterService;
        private DateTime? _Sysdate;
        private readonly SysContextOptions _options;
        private readonly ISpCollectService _spCollectService;
        private readonly IRepairPlanService _repairPlanService;

        public OAService(IDbContext dbContext,UserSession userSession, IMessageCenterService messageCenterService, IOptions<SysContextOptions> sysContext,
            ISpCollectService spCollectsService, IRepairPlanService repairPlanService)
        {
            _dbContext = dbContext;
            _userSession = userSession;
            _messageCenterService = messageCenterService;
            _options = sysContext.Value;
            _spCollectService = spCollectsService;
            _repairPlanService = repairPlanService;
        }

        /// <summary>
        /// 获取数据库时间
        /// </summary>
        private DateTime? Sysdate
        {
            get
            {
                if (!_Sysdate.HasValue)
                {
                    _Sysdate = _dbContext.GetSysdate().Result();
                }
                return _Sysdate;
            }
        }

        public async Task<bool> LogAsync(string type, string paras, string detail)
        {
            await _dbContext.DBLog(type, "", paras + "\n" + detail, "");
            return true;
        }

        //接口返回参数
        private class RETURN_PARAM
        {
            public bool status;
            public string msg;
        }

        /// <summary>
        /// oa 回调数据
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<string> GetOADataAsync(dynamic data)
        {
            await _dbContext.DBLog("OA创建流程", "", $"收到OA回调请求，回调参数：\n{data}", "");

            var returnParam = new RETURN_PARAM
            {
                status = true,
                msg = "回调成功"
            };

            #region data 格式
            /* { 
             * taskId:1,
             * isFinish:false,
             * primary_key: 1735590940259123200,
             * fun_name: "_spCollectService.ApprovalCompletedAsync",
             * detail:[{
             *      operation:"提交",//"退回"
             *      memo:"意见",
             *      receiveTime:"2021-10-01 10:10:10",//任务接收时间
             *      dealTime:"2021-10-01 10:10:10",//任务处理时间
             *      delUser:"任务处理人",
             *      nodeName:"审批节点",
             *      isReback:false
             *      }]
             *  }
             */
            #endregion

            if (data == null)
            {
                returnParam.status = false;
                returnParam.msg = "回调参数为空串";
                return returnParam.ToJson();
            }
            string json = JsonConvert.SerializeObject(data);

            //判断必须有内容的参数
            JObject jObj = JObject.Parse(json);
            var checkValidParams = new string[] { "taskId", "primary_key", "fun_name", "detail" };
            foreach (string param in checkValidParams)
            {
                if (!jObj.ContainsKey(param) || string.IsNullOrEmpty(jObj.GetValue(param).ToString()))
                {
                    returnParam.status = false;
                    returnParam.msg = $"回调参数{param}缺少值";
                    return returnParam.ToJson();
                }
            }

            //启用事务
            var canTransationOper = false;
            try
            {
                canTransationOper = !_dbContext.Session.IsInTransaction;
                if (canTransationOper)
                {
                    _dbContext.Session.BeginTransaction();
                }

                var item = jObj["detail"]?[0] ?? JObject.Parse("");

                //插入流程记录
                await _dbContext.InsertAsync(new WF_PROCESS
                {
                    PROCESS_ID = GuidHelper.NewSnowflakeId().ToString(),
                    TASK_ID = jObj["taskId"]?.ToString() ?? "",
                    OPERATION = item["operation"]?.ToString() ?? "",
                    MEMO = item["memo"]?.ToString() ?? "",
                    RECEIVE_TIME = Convert.ToDateTime(item["receiveTime"]?.ToString()),
                    DEAL_TIME = Convert.ToDateTime(item["dealTime"]?.ToString()),
                    DEAL_USER = item["delUser"]?.ToString() ?? "",
                    NODE_NAME = item["nodeName"]?.ToString() ?? "",
                    IS_REBACK = item["isReback"]?.ToString() == "true" ? "1" : "0"
                });
                if (canTransationOper) _dbContext.Session.CommitTransaction();

                //调用物资采购的回调函数
                var isPass = item["operation"]?.ToString() == "提交"; //!= "退回"
                var fun_name = jObj["fun_name"]?.ToString();
                switch (fun_name)
                {
                    case "_spCollectService.ApprovalCompletedAsync":
                        await _spCollectService.ApprovalCompletedAsync(jObj["primary_key"]?.ToString(), isPass);
                        break;
                    case "_repairPlanService.ApprovalCompletedAsync":
                        await _repairPlanService.ApprovalCompletedAsync(jObj["primary_key"]?.ToString(), isPass);
                        break;
                    default:
                        throw new MessageException("未找到匹配的回调函数");
                }
            }
            catch (Exception ex)
            {
                returnParam.status = false;
                returnParam.msg = "回调异常：" + ex.Message;
            }
            finally
            {
                if (canTransationOper && _dbContext.Session.IsInTransaction)
                {
                    _dbContext.Session.RollbackTransaction();
                }
            }

            await _dbContext.DBLog("OA创建流程", "", "OA回调结束", returnParam.ToJson());
            return returnParam.ToJson();
        }

        public string RemoveHtml(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return "";
            }
            html = Regex.Replace(html, "<[^>]+>", "");
            html = Regex.Replace(html, "&[^;]+;", "");
            return html;
        }


    }
}
