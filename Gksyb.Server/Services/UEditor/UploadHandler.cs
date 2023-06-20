using Microsoft.AspNetCore.Http;
using System.IO;

namespace Gksyb.Server.Services.UEditor
{
    public class UploadHandler : Handler
    {
        public UploadConfig UploadConfig { get; private set; }
        public UploadResult Result { get; private set; }

        public UploadHandler(UploadConfig config)
        {
            UploadConfig = config;
            Result = new UploadResult() { State = UploadState.Unknown };
        }

        public override object Process()
        {
            MemoryStream stream = null;
            try
            {
                IFormFile file;
                if (UploadConfig.Base64)
                {
                    stream = new MemoryStream(Convert.FromBase64String(Request.GetParm(UploadConfig.UploadFieldName)));
                    file = new FormFile(stream, 0, stream.Length, UploadConfig.Base64Filename, UploadConfig.Base64Filename);
                }
                else
                {
                    file = Request.Form.Files[UploadConfig.UploadFieldName];
                }

                if (!CheckFileType(file.FileName))
                {
                    Result.State = UploadState.TypeNotAllow;
                    return WriteResult();
                }
                if (!CheckFileSize(file.Length))
                {
                    Result.State = UploadState.SizeLimitExceed;
                    return WriteResult();
                }

                file.CheckImageSecurity();
                Result.OriginFileName = file.FileName;

                var savePath = PathFormatter.Format(file.FileName, UploadConfig.PathFormat);
                var localPath = Path.Combine(Config.Environment.WebRootPath, Path.Combine(savePath.TrimStart('/').Split("/")));
                if (!localPath.Contains(Path.Combine(Config.Environment.WebRootPath, Config.UploadDirectory)))
                {
                    Result.State = UploadState.FileAccessError;
                    return WriteResult();
                }
                try
                {
                    if (!Directory.Exists(Path.GetDirectoryName(localPath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(localPath));
                    }
                    file.Save(localPath, savePath).Result();
                    Result.Url = savePath;
                    Result.State = UploadState.Success;
                }
                catch (Exception e)
                {
                    Result.State = UploadState.FileAccessError;
                    Result.ErrorMessage = e.Message;
                }
                return WriteResult();
            }
            finally
            {
                stream?.Dispose();
            }
        }

        private object WriteResult()
        {
            return new
            {
                state = GetStateMessage(Result.State),
                url = Result.Url,
                title = Result.OriginFileName,
                original = Result.OriginFileName,
                error = Result.ErrorMessage
            };
        }

        private string GetStateMessage(UploadState state)
        {
            return state switch
            {
                UploadState.Success => "SUCCESS",
                UploadState.FileAccessError => "文件访问出错，请检查写入权限",
                UploadState.SizeLimitExceed => string.Format("文件压缩后超过{0}M", (UploadConfig.SizeLimit / 1024 / 1024)),
                UploadState.TypeNotAllow => "不允许的文件格式",
                UploadState.NetworkError => "网络错误",
                _ => "未知错误",
            };
        }

        private bool CheckFileType(string filename)
        {
            var fileExtension = Path.GetExtension(filename).ToLower();
            return UploadConfig.AllowExtensions.Select(x => x.ToLower()).Contains(fileExtension);
        }

        private bool CheckFileSize(long size)
        {
            return size < UploadConfig.SizeLimit;
        }
    }

    public class UploadConfig
    {
        /// <summary>
        /// 文件命名规则
        /// </summary>
        public string PathFormat { get; set; }

        /// <summary>
        /// 上传表单域名称
        /// </summary>
        public string UploadFieldName { get; set; }

        /// <summary>
        /// 上传大小限制
        /// </summary>
        public long SizeLimit { get; set; }

        /// <summary>
        /// 上传允许的文件格式
        /// </summary>
        public string[] AllowExtensions { get; set; }

        /// <summary>
        /// 文件是否以 Base64 的形式上传
        /// </summary>
        public bool Base64 { get; set; }

        /// <summary>
        /// Base64 字符串所表示的文件名
        /// </summary>
        public string Base64Filename { get; set; }
    }

    public class UploadResult
    {
        public UploadState State { get; set; }
        public string Url { get; set; }
        public string OriginFileName { get; set; }

        public string ErrorMessage { get; set; }
    }

    public enum UploadState
    {
        Success = 0,
        SizeLimitExceed = -1,
        TypeNotAllow = -2,
        FileAccessError = -3,
        NetworkError = -4,
        Unknown = 1,
    }
}