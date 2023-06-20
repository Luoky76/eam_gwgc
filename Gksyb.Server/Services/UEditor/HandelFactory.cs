using Microsoft.AspNetCore.Http;

namespace Gksyb.Server.Services.UEditor
{
    public class HandelFactory
    {
        public static Handler GetHandler(HttpContext context)
        {
            var action = context.Request.GetParm("action");
            switch (action)
            {
                case "config":
                    return new ConfigHandler();

                case "uploadimage":
                    string imagePath = context.Request.GetParm("imagePath");
                    return new UploadHandler(new UploadConfig()
                    {
                        AllowExtensions = Config.GetStringList("imageAllowFiles"),
                        PathFormat = string.IsNullOrEmpty(imagePath) ? Config.GetString("imagePathFormat") : "/UploadDirectory/UeditorUpload/image/" + imagePath + "/{yyyy}{mm}{dd}/{time}{rand:6}",
                        SizeLimit = Config.GetInt("imageMaxSize"),
                        UploadFieldName = Config.GetString("imageFieldName")
                    });

                case "uploadscrawl":
                    return new UploadHandler(new UploadConfig()
                    {
                        AllowExtensions = new string[] { ".png" },
                        PathFormat = Config.GetString("scrawlPathFormat"),
                        SizeLimit = Config.GetInt("scrawlMaxSize"),
                        UploadFieldName = Config.GetString("scrawlFieldName"),
                        Base64 = true,
                        Base64Filename = "scrawl.png"
                    });

                case "uploadvideo":
                    return new UploadHandler(new UploadConfig()
                    {
                        AllowExtensions = Config.GetStringList("videoAllowFiles"),
                        PathFormat = Config.GetString("videoPathFormat"),
                        SizeLimit = Config.GetInt("videoMaxSize"),
                        UploadFieldName = Config.GetString("videoFieldName")
                    });

                case "uploadfile":
                    return new UploadHandler(new UploadConfig()
                    {
                        AllowExtensions = Config.GetStringList("fileAllowFiles"),
                        PathFormat = Config.GetString("filePathFormat"),
                        SizeLimit = Config.GetInt("fileMaxSize"),
                        UploadFieldName = Config.GetString("fileFieldName")
                    });

                case "listimage":
                    return new ListFileManager(Config.GetString("imageManagerListPath"), Config.GetStringList("imageManagerAllowFiles"));

                case "listfile":
                    return new ListFileManager(Config.GetString("fileManagerListPath"), Config.GetStringList("fileManagerAllowFiles"));

                default:
                    return new NotSupportedHandler();
            }
        }
    }
}