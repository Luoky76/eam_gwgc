using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Gksyb.Server.Services.UEditor
{
    public static class Config
    {
        static Config()
        {
            var requestServices = Gksyb.Common.Static.HttpContext.RequestServices;
            Environment = requestServices.GetService<IWebHostEnvironment>();
            var json = File.ReadAllText(Path.Combine(Environment.ContentRootPath, "Config", "ueditor.json"));
            Items = JObject.Parse(json);
            UploadDirectory = requestServices.GetService<IConfiguration>().GetValue<string>(OptionName.UploadDirectory);
        }

        public static JObject Items { get; private set; }

        public static T GetValue<T>(string key)
        {
            return Items[key].Value<T>();
        }

        public static string[] GetStringList(string key)
        {
            return Items[key].Select(x => x.Value<string>()).ToArray();
        }

        public static string GetString(string key)
        {
            return GetValue<string>(key);
        }

        public static int GetInt(string key)
        {
            return GetValue<int>(key);
        }

        public static IWebHostEnvironment Environment { get; private set; }

        public static string UploadDirectory { get; private set; }
    }
}