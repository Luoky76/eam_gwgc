namespace Gksyb.Server.Services.UEditor
{
    /// <summary>
    /// 配置处理
    /// </summary>
    public class ConfigHandler : Handler
    {
        public override object Process()
        {
            return Config.Items;
        }
    }
}