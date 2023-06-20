using System;
using System.IO;

namespace WebHost
{
    public static class StaticData
    {
        /// <summary>
        /// 插件路径
        /// </summary>
        public static readonly string PluginDirectory = Path.Combine(AppContext.BaseDirectory, "plugins");
    }
}