using Quartz.Impl.Triggers;

namespace Quartz
{
    public static class QuartzExtension
    {
        /// <summary>
        /// 验证cron是否正确
        /// </summary>
        /// <returns></returns>
        public static bool IsValidCron(this string source)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(source)) return true;
                var trigger = new CronTriggerImpl
                {
                    CronExpressionString = source
                };
                return trigger.ComputeFirstFireTimeUtc(null) != null;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}