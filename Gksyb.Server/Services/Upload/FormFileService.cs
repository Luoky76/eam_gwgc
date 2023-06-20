using Gksyb.Common.Mvc.Interface;
using Gksyb.Core.Auth;
using Gksyb.Model.Core;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace Gksyb.Server.Services.Upload
{
    public class FormFileService : IFormFileService
    {
        private readonly IDbContext _dbContext;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly UserSession _user;

        public FormFileService(IDbContext dbContext, UserSession user, IHttpContextAccessor contextAccessor)
        {
            _dbContext = dbContext;
            _contextAccessor = contextAccessor;
            _user = user;
        }

        public async Task Save(string url, string path, IFormFile formFile)
        {
            var hash = await formFile.GetHashAsync();
            var file = new SYS_FILE()
            {
                ID = GuidHelper.NewSnowflakeId(),
                FILE_HASH = hash,
                FILE_NAME = Path.GetFileName(path),
                FILE_PATH = path,
                FILE_URL = url,
                ORGIN_FILE_NAME = formFile.FileName,
                FILE_SIZE = formFile.Length,
                FILE_TYPE = formFile.GetContentType(),
                CREATEDATE = await _dbContext.GetSysdate()
            };
            if (_contextAccessor != null)
            {
                var request = _contextAccessor.HttpContext.Request;
                file.IP = request.GetRealIP();
                file.SOURCE_URL = request.GetRealUrl();
                file.SOURCE_PATH = request.RouteValues?.Count > 0 ? request.RouteValues.Values.Join("/") : request.Path;
                file.CREATEUSERID = _user.UserID;
                file.CREATEUSER = _user.RealName;
            }
            await _dbContext.InsertAsync(file);
        }
    }
}