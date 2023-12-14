using Newtonsoft.Json.Linq;
using Flurl;
using Flurl.Http;

namespace Gksyb.Core.OA
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
        public static async Task<string> GetUserIdAsync(string phone,string url)
        {
            string paras = "{\"mobile\":\"" + phone + "\"}";
            try
            {
                var http = new Url(url.TrimEnd('/') + "/hrm/HrmUserId/getUserId");
                string result = await http.PostJsonAsync(paras).ReceiveString();

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


        public async Task<string> CreateFlow(string url, string billId, string billTitle, string phone, string oaCode, string mainData, string detData)
        {

            
            string json = await GetFlowJsonAsync(url, billId, billTitle, phone, oaCode, mainData, detData);

            await _dbContext.DBLog("创建OA流程参数", "", "手机号码：" + phone + "====OA账号：" + oaCode + "\n" + json,"");

            //通过tojson转化的json  会含有null 的数据 需要替换成 ""    特殊处理掉日期带时间的问题
            json = json.Replace(":null", ":\"\"").Replace(" 00:00:00", "");

            var http = new Url(url.TrimEnd('/') + "/createWorkflow?method=getExternalData");
            string result = await http.PostJsonAsync(json).ReceiveString();

            await _dbContext.DBLog("创建OA流程结果", "", result + "\n" + json, "");
            return result;
        }


        /// <summary>
        /// 拼接oa接口需求参数
        /// </summary>
        /// <param name="url"></param>
        /// <param name="billId"></param>
        /// <param name="billTitle"></param>
        /// <param name="phone"></param>
        /// <param name="oaCode"></param>
        /// <param name="mainData"></param>
        /// <param name="detData"></param>
        /// <returns></returns>
        public async Task<string> GetFlowJsonAsync(string url, string billId, string billTitle, string phone, string oaCode, string mainData, string detData)
        {
            #region 拼接json
            string dataJson = string.Empty, loginid = oaCode;

            if (string.IsNullOrEmpty(loginid))
            {
                loginid = await GetUserIdAsync(phone, url);
            }

            if (string.IsNullOrEmpty(loginid))
            {
                return "未成功获取到oa登录id";
            }

            StringBuilder sb = new StringBuilder();
            //json 开始
            sb.Append("{");

            // 流程信息
            sb.Append("\"billId\":\"" + billId + "\",");
            sb.Append("\"billTitle\":\"" + billTitle + "\",");
            sb.Append("\"creator\":\"" + loginid + "\",");
            sb.Append("\"isAutoSubmit\":\"0\",");
            sb.Append("\"oaRequest\":\"\",");

            // mainTable 主表信息 开始
            sb.Append("\"mainTable\":");

            sb.Append(mainData);// mainData 格式已自带 {}  
            // mainTable 主表信息 结束
            sb.Append(",");

            // detailTable 子表信息 开始
            sb.Append("\"detailTable\":");

            sb.Append(detData);// detData 格式已自带 {}  
            // detailTable 子表信息 结束

            //json 结束
            sb.Append("}");
            #endregion

            return sb.ToString().Replace(":null", ":\"\"").Replace(" 00:00:00", "");
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
