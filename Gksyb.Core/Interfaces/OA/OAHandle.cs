using Newtonsoft.Json.Linq;
using Flurl;
using Flurl.Http;
using DocumentFormat.OpenXml.EMMA;
using Flurl.Http.Content;
using Newtonsoft.Json;

namespace Gksyb.Core.Interfaces.OA
{
    public class OAHandle
    {
        protected readonly IDbContext _dbContext;

        public OAHandle(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// 获取账号
        /// </summary>
        /// <param name="phone">手机号码</param>
        /// <param name="url">oa地址</param>
        /// <returns></returns>
        public static async Task<string> GetUserIdAsync(string phone, string url)
        {
            var paras = new
            {
                mobile = phone
            };
            try
            {
                var content = new CapturedJsonContent(paras.ToJson());
                string result = await (url.TrimEnd('/') + "/hrm/HrmUserId/getUserId").PostAsync(content).ReceiveString();

                //解析json 
                JObject jObj = JObject.Parse(result);
                if (jObj["loginid"] != null)
                {
                    return jObj["loginid"].ToString();
                }
                return "";
            }
            catch (Exception ex)
            {
                return "error01：调用获取用户接口异常," + ex.Message;
            }
        }


        public async Task<string> CreateFlow(string url, string billId, string billTitle, string phone, string loginid, Object mainData, Object detData)
        {
            string json = await GetFlowJsonAsync(url, billId, billTitle, phone, loginid, mainData, detData);
            await _dbContext.DBLog("创建OA流程参数", "", $"手机号码：{phone}\nOA账号：{loginid}\n{json}", "");
            var content = new CapturedJsonContent(json);
            string result = await ($"{url.TrimEnd('/')}/createWorkflow?method=getExternalData").PostAsync(content).ReceiveString();
            await _dbContext.DBLog("创建OA流程结果", "", result, "");
            return result;
        }

        /// <summary>
        /// 拼接oa接口需求参数
        /// </summary>
        /// <param name="url"></param>
        /// <param name="billId"></param>
        /// <param name="billTitle"></param>
        /// <param name="phone"></param>
        /// <param name="loginid">为空时将通过phone向OA请求得到</param>
        /// <param name="mainData"></param>
        /// <param name="detData"></param>
        /// <returns></returns>
        public async Task<string> GetFlowJsonAsync(string url, string billId, string billTitle, string phone, string loginid, Object mainData, Object detData)
        {
            if (string.IsNullOrEmpty(loginid))
            {
                loginid = await GetUserIdAsync(phone, url);
            }

            if (string.IsNullOrEmpty(loginid))
            {
                return "未成功获取到oa登录id";
            }

            var queryParams = new
            {
                billId,
                billTitle,
                creator = loginid,
                isAutoSubmit = "0",
                oaRequest = "",
                mainTable = mainData,
                detailTable = detData
            };

            return queryParams.ToJson();
        }

        public async Task<string> GetOALogList(string url, string tid, string oaid, string json)
        {
            await _dbContext.DBLog("获取OA实时审批进度", "", "TASKid：" + tid + "====OAid：" + oaid + "\n" + json, "");

            //通过tojson转化的json  会含有null 的数据 需要替换成 ""    特殊处理掉日期带时间的问题
            json = json.Replace(":null", ":\"\"").Replace(" 00:00:00", "");

            var http = new Url(url.TrimEnd('/') + "/cusrequest/getrequestlog/getList");
            string result = await http.PostJsonAsync(json).ReceiveString();

            await _dbContext.DBLog("获取OA实时审批进度结果", "", result + "\n" + json, "");
            return result;
        }

    }
}
