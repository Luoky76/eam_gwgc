using Chloe;
using Gksyb.Common;
using Gksyb.Core.Auth;
using Gksyb.Core.Grid;
using Gksyb.Core.Interfaces.Common;
using Gksyb.Model.Core;
using Gksyb.Model.Grid;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gksyb.Server.Services.Common
{
    /// <summary>
    /// 附件服务
    /// </summary>
    public class AttachService : IAttachService
    {
        private readonly IDbContext _dbContext;
        protected UserSession CurrentUser;
        private DateTime? _Sysdate;
        protected IWebHostEnvironment _webhost;

        /// <summary>
        /// 附件服务
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="webhost"></param>
        public AttachService(IDbContext dbContext, IWebHostEnvironment webhost)
        {
            _dbContext = dbContext;
            _webhost = webhost;
        }

        /// <summary>
        /// 获取缓存用户
        /// </summary>
        /// <param name="user"></param>
        public void SetUser(UserSession user)
        {
            CurrentUser = user;
        }

        /// <summary>
        /// 上传附件
        /// </summary>
        /// <param name="formFile"></param>
        /// <param name="parms"></param>
        /// <returns></returns>
        public async Task<AjaxResult> Upload([FileOptions("gif,jpg,jpeg,bmp,png,pdf,xlsx,xls,doc,docx", 200)] IFormFile formFile, string parms)
        {
            JObject jo = JObject.Parse(parms);
            if (jo.Property("tableName") == null || jo.Property("dataId") == null || jo.Property("fileName") == null || jo.Property("attachField") == null)
            {
                throw new Exception("系统后台接收参数出错！请联系公司相关人员处理！");
            }
            string tableName = jo["tableName"].ToString();
            string dataId = jo["dataId"].ToString();
            string newFileName = jo["fileName"].ToString();
            string attachField = jo["attachField"].ToString();

            if (dataId.IsNullOrEmpty() || tableName.IsNullOrEmpty())
            {
                throw new Exception("未将附件绑定到项目！");
            }

            long fileSize = formFile.Length;
            string fileType = formFile.ContentType;
            string fileName = formFile.FileName;
            string fileExtension = Regex.Match(formFile.FileName, @"\..+$").ToString().ToLower();
            var oldfile = fileName.Split(".");
            newFileName += $".{oldfile[1]}";

            //以newFileName存入tableName文件夹
            var path = await formFile.SaveAs(tableName, newFileName, true);

            //var requestServices = Gksyb.Common.Static.HttpContext.Current.RequestServices;
            //var webhost = requestServices.GetRequiredService<IWebHostEnvironment>();
            var fullpath = _webhost.WebRootPath + path;

            //if (!MimeHelper.GetMimeMapping(fileName).Contains("image")) throw new Exception("上传的附件不是图片格式的！请重新上传！");

            //DateTime? sysdate = SysContext.Sysdate;
            //SYS_ATTACH attach = new SYS_ATTACH();
            //attach.ATTACH_ID = GuidHelper.NewSnowflakeId().ToString();
            //attach.DATA_ID = dataId;
            //attach.ATTACH_NAME = fileName;
            //attach.ATTACH_FIELD = attachField;
            //attach.ATTACH_PATH = filePath.Replace("\\", "/");
            //attach.ATTACH_URLPATH = serverFilePath.Replace("\\", "/");
            //attach.TABLE_NAME = tableName;
            //attach.FILE_SIZE = fileSize;
            //attach.CONTENT_TYPE = fileType;
            //attach.ATTACH_TYPE = fileExtension;
            //attach.UPLOAD_USER = SysContext.CurrentUserName;
            //attach.UPLOAD_DATE = sysdate;
            //attach.CREATEUSER = SysContext.CurrentUserName;
            //attach.CREATEDATE = sysdate;

            //dbTransaction.Insert<SYS_ATTACH>(attach);
            await _dbContext.InsertAsync(() => new SYS_ATTACH()
            {
                attach_id = GuidHelper.NewShortId(),
                data_id = dataId,
                attach_name = fileName,
                attach_field = attachField,
                attach_path = fullpath.Replace(@"\\", @"/"),
                attach_urlpath = path.Replace(@"\\", @"/"),
                table_name = tableName,
                file_size = fileSize,
                content_type = fileType,
                attach_type = fileExtension,
                upload_user = CurrentUser.RealName,
                upload_date = Sysdate,
                createuser = CurrentUser.RealName,
                createdate = Sysdate
            });

            return AjaxResult.Success(fileName, path);
        }

        /// <summary>
        /// 获取某单据关联的各类附件数量
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="dataId"></param>
        /// <returns></returns>
        public async Task<AjaxResult> GetAttachFieldStat(string tableName, string dataId)
        {
            if (tableName.IsNullOrEmpty()) throw new Exception("未获取到表名称！");
            if (dataId.IsNullOrEmpty()) throw new Exception("未获取到单据ID！");

            var qryStatSql = $"select attach_field, count(1) cnt from sys_attach  where table_name = '{tableName}' and data_id = '{dataId}' group by attach_field";

            var attachList = await _dbContext.SqlQueryAsync<dynamic>(qryStatSql);

            return AjaxResult.Success(attachList, "获取成功！");
        }

        /// <summary>
        /// 获取附件列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<GridData> ListAsync(GridRequest request)
        {
            return await _dbContext.Query<SYS_ATTACH>().Where(c => 1 == 1).Select(c => new
            {
                c.attach_id,
                c.table_name,
                c.data_id,
                c.attach_name,
                c.content_type,
                c.attach_field,
                c.fun_id,
                c.fun_name,
                c.upload_user,
                c.upload_date,
                c.attach_path,
                c.attach_type,
                c.file_size,
                c.createuser,
                c.createdate,
                c.modifyuser,
                c.modifydate,
                c.attach_urlpath
            }).GetGridData(request);
        }

        /// <summary>
        /// 删除附件（根据附件ID）
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<AjaxResult> DeleteAttachByAttachId(string id)
        {
            if (String.IsNullOrEmpty(id)) throw new MessageException("未获取到附件ID！");
            var data = await _dbContext.Query<SYS_ATTACH>().Where(c => c.attach_id == id).FirstOrDefaultAsync();

            if (data != null)
            {
                string path = data.attach_path;
                //LogHelper.WriteLog("附件删除开始：" + path);

                //删除文件
                if (File.Exists(path))
                {
                    FileInfo fi = new FileInfo(path);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1) fi.Attributes = FileAttributes.Normal;
                    File.Delete(path);
                }
                else
                {
                    //LogHelper.WriteLog("找不到文件：" + path);
                }

                //删除附件记录
                if (await _dbContext.DeleteAsync<SYS_ATTACH>(data) != 1)
                {
                    //LogHelper.WriteLog("删除记录出错！");
                    throw new MessageException("删除记录出错！");
                }

                //返回结果
                //LogHelper.WriteLog("附件删除完成！");
                return AjaxResult.Success("删除成功！");
            }
            else
            {
                //LogHelper.WriteLog("未找到相关附件记录！");
                throw new MessageException("未找到相关附件记录！");
            }
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
                    _Sysdate = _dbContext.GetSysdate().GetAwaiter().GetResult();
                }
                return _Sysdate;
            }
        }
    }
}