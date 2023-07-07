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

        public async Task<string> SaveAsync(string url, string path, string mapPath, IFormFile formFile)
        {
            var hash = await formFile.GetHashAsync();
            var directory = Path.GetDirectoryName(path).Trim(Path.DirectorySeparatorChar);
            var file = await _dbContext.Query<SYS_FILE>().Where(c => c.FILE_HASH == hash && c.FILE_PATH == directory).OrderByDesc(c => c.ID).FirstOrDefaultAsync();
            if (!string.IsNullOrWhiteSpace(mapPath) && file != null)
            {
                var fullPath = Path.GetFullPath(Path.Combine(mapPath, file.FILE_PATH, file.FILE_NAME));
                if (!File.Exists(fullPath))
                {
                    file = null;
                }
            }
            file ??= new SYS_FILE()
            {
                FILE_HASH = hash,
                FILE_NAME = Path.GetFileName(path),
                FILE_PATH = directory,
                FILE_URL = url
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