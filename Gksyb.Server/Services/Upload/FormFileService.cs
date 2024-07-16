using Gksyb.Common.Mvc.Dtos;
using Gksyb.Common.Mvc.Interface;
using Gksyb.Core.Auth;
using Gksyb.Model.Core;
using Microsoft.AspNetCore.Http;

namespace Gksyb.Server.Services.Upload
{
    public class FormFileService : IFormFileService
    {
        private readonly IDbContext _dbContext;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ScopeUser _user;

        public FormFileService(IDbContext dbContext, ScopeUser user, IHttpContextAccessor contextAccessor)
        {
            _dbContext = dbContext;
            _contextAccessor = contextAccessor;
            _user = user;
        }

        public async Task<string> SaveAsync(FormFileRequest fileRequest)
        {
            var formFile = fileRequest.FormFile;
            var hash = await formFile.GetHashAsync();
            var directory = Path.GetDirectoryName(fileRequest.Path).Trim(Path.DirectorySeparatorChar);
            var file = fileRequest.IgnoreHash ? null :
                await _dbContext.Query<SYS_FILE>().Where(c => c.FILE_HASH == hash && c.FILE_PATH == directory).OrderByDesc(c => c.ID).FirstOrDefaultAsync();
            if (!string.IsNullOrWhiteSpace(fileRequest.MapPath) && file != null)
            {
                var fullPath = Path.GetFullPath(Path.Combine(fileRequest.MapPath, file.FILE_PATH, file.FILE_NAME));
                if (!File.Exists(fullPath))
                {
                    file = null;
                }
            }
            file ??= new SYS_FILE()
            {
                FILE_HASH = hash,
                FILE_NAME = Path.GetFileName(fileRequest.Path),
                FILE_PATH = directory,
                FILE_URL = fileRequest.Url
            };
            file.ID = GuidHelper.NewSnowflakeId();
            file.ORGIN_FILE_NAME = formFile.FileName;
            file.FILE_SIZE = formFile.Length;
            file.FILE_TYPE = formFile.GetContentType();
            file.CREATEDATE = await _dbContext.GetSysdate();
            if (_contextAccessor != null)
            {
                var request = _contextAccessor.HttpContext.Request;
                file.IP = _user.IP;
                file.SOURCE_URL = request.GetRealUrl();
                file.SOURCE_PATH = request.RouteValues?.Count > 0 ? request.RouteValues.Values.Join("/") : request.Path;
                file.CREATEUSERID = _user.UserID;
                file.CREATEUSER = _user.RealName;
            }
            await _dbContext.InsertAsync(file);
            return file.FILE_URL;
        }
    }
}