using System.Text;

namespace System
{
    public class MessageException : Exception
    {
        /// <summary>
        /// 消息异常 不跟踪堆栈
        /// </summary>
        /// <param name="message"></param>
        public MessageException(string message) : base(message)
        {
        }

        /// <summary>
        /// 重写转字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Message;
        }

        /// <summary>
        /// 抛出消息异常
        /// </summary>
        public static void Throw(string message)
        {
            throw new MessageException(message);
        }

        /// <summary>
        /// 抛出消息异常
        /// </summary>
        public static void ThrowIf(bool isTrow, string message)
        {
            if (isTrow) throw new MessageException(message);
        }
    }

    /// <summary>
    /// 异常操作扩展
    /// </summary>
    public static class ExceptionExtensions
    {
        /// <summary>
        /// 格式化异常消息
        /// </summary>
        /// <param name="ex">异常对象</param>
        /// <param name="isHideStackTrace">是否隐藏异常规模信息</param>
        /// <param name="isFinal"></param>
        /// <returns>格式化后的异常信息字符串</returns>
        public static string FormatMessage(this Exception ex, bool isHideStackTrace = false, bool isFinal = true)
        {
            try
            {
                var e = ex;
                if (isFinal)
                {
                    while (e.InnerException != null)
                    {
                        e = e.InnerException;
                    }
                }
                if (e is MessageException) return e.Message;
                var sb = new StringBuilder();
                int count = 0;
                string appString = string.Empty;
                while (e != null)
                {
                    if (count > 0)
                    {
                        appString += "  ";
                    }
                    sb.AppendLine($"{appString}异常消息：{e.Message}");
                    sb.AppendLine($"{appString}异常类型：{e.GetType().FullName}");
                    sb.AppendLine($"{appString}异常方法：{e.TargetSite?.Name}");
                    sb.AppendLine($"{appString}异常源：{e.Source}");
                    if (!isHideStackTrace && e.StackTrace != null)
                    {
                        sb.AppendLine($"{appString}异常堆栈：{e.StackTrace}");
                    }
                    if (e.InnerException != null)
                    {
                        sb.AppendLine($"{appString}内部异常：");
                        count++;
                    }
                    e = e.InnerException;
                }
                return sb.ToString();
            }
            catch (Exception)
            {
                return ex.Message;
            }
        }
    }
}