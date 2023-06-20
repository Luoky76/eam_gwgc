using System.IO;

namespace Gksyb.Server.Services.UEditor
{
    public class ListFileManager : Handler
    {
        private enum ResultState
        {
            Success,
            InvalidParam,
            AuthorizError,
            IOError,
            PathNotFound
        }

        private int Start;
        private int Size;
        private int Total;
        private ResultState State;
        private readonly string PathToList;
        private string[] FileList;
        private readonly string[] SearchExtensions;

        public ListFileManager(string pathToList, string[] searchExtensions)
        {
            this.SearchExtensions = searchExtensions.Select(x => x.ToLower()).ToArray();
            this.PathToList = pathToList;
        }

        public override object Process()
        {
            try
            {
                Start = string.IsNullOrEmpty(Request.GetParm("start")) ? 0 : Convert.ToInt32(Request.GetParm("start"));
                Size = string.IsNullOrEmpty(Request.GetParm("size")) ? Config.GetInt("imageManagerListSize") : Convert.ToInt32(Request.GetParm("size"));
            }
            catch (FormatException)
            {
                State = ResultState.InvalidParam;
                return WriteResult();
            }
            var buildingList = new List<string>();
            try
            {
                var localPath = Path.Combine(Config.Environment.WebRootPath, Path.Combine(PathToList.TrimStart('/').Split("/")));
                if (!localPath.Contains(Path.Combine(Config.Environment.WebRootPath, Config.UploadDirectory)))
                {
                    throw new DirectoryNotFoundException();
                }
                buildingList.AddRange(Directory.GetFiles(localPath, "*", SearchOption.AllDirectories)
                    .Where(x => SearchExtensions.Contains(Path.GetExtension(x).ToLower()))
                    .Reverse()
                    .Select(x => PathToList + x[localPath.Length..].Replace("\\", "/")));
                Total = buildingList.Count;
                FileList = buildingList.Skip(Start).Take(Size).ToArray();
            }
            catch (UnauthorizedAccessException)
            {
                State = ResultState.AuthorizError;
            }
            catch (DirectoryNotFoundException)
            {
                State = ResultState.PathNotFound;
            }
            catch (IOException)
            {
                State = ResultState.IOError;
            }
            return WriteResult();
        }

        private object WriteResult()
        {
            return new
            {
                state = GetStateString(),
                list = FileList?.Select(x => new { url = x }),
                start = Start,
                size = Size,
                total = Total
            };
        }

        private string GetStateString()
        {
            return State switch
            {
                ResultState.Success => "SUCCESS",
                ResultState.InvalidParam => "参数不正确",
                ResultState.PathNotFound => "路径不存在",
                ResultState.AuthorizError => "文件系统权限不足",
                ResultState.IOError => "文件系统读取错误",
                _ => "未知错误",
            };
        }
    }
}