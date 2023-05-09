using Gksyb.Common.Quartz;
using Gksyb.Common.Weixin;
using Gksyb.Model.Core;

namespace Gksyb.Server.Job.NoticeHandle
{
    /// <summary>
    /// 微信通知
    /// </summary>
    public class WeixinNoticeHandle : IBaseService, INoticeHandle
    {
        private readonly IDbContext _dbContext;

        public WeixinNoticeHandle(IDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Excute(string type, string error)
        {
            var tdata = new WeiXinTemplateData
            {
                first = new WeiXinTemplateDataItem("带颜色数据", "#FF0000"),
                keyword1 = new WeiXinTemplateDataItem("keyword1"),
                keyword2 = new WeiXinTemplateDataItem("keyword2"),
                keyword3 = new WeiXinTemplateDataItem("keyword3"),
                keyword4 = new WeiXinTemplateDataItem("keyword4"),
                keyword5 = new WeiXinTemplateDataItem("keyword5"),
                keyword6 = new WeiXinTemplateDataItem("keyword6"),
                keyword7 = new WeiXinTemplateDataItem("keyword7"),
                remark = new WeiXinTemplateDataItem(error)
            };
            var notice = new WEIXIN_NOTICE
            {
                SID = GuidHelper.NewShortId(),
                STATUS = "未发送",
                TEMPLATE = "KxNcvKO-uMSa9kbKTKjCGAIK85c3-TLP6apXKwwvJ-Y",
                RECEIVER = "o-b84013U7BKLldCHA6mIrbQEd3U",
                URL = "https://www.baidu.com/",
                TDATA = tdata.ToJson(),
                CREATEUSER = "创建人",
                CREATEDATE = DateTime.Now,
                MODIFYUSER = "修改人",
                MODIFYDATE = DateTime.Now
            };
            await _dbContext.InsertAsync(notice);
        }
    }
}