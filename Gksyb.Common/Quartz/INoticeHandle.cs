namespace Gksyb.Common.Quartz
{
    /// <summary>
    /// 通知
    /// </summary>
    public interface INoticeHandle
    {
        /// <summary>
        /// 通知
        /// </summary>
        /// <returns></returns>
        public Task Excute(string type, string error);
    }
}